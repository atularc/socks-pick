using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using SockPairs.Core;

class Program
{
    static int assertions;
    static void Check(bool condition,string message) { assertions++; if(!condition) throw new Exception(message); }
    static void Main()
    {
        var book=JsonSerializer.Deserialize<LevelBook>(File.ReadAllText("Assets/Resources/Data/levels.json"),new JsonSerializerOptions{IncludeFields=true});
        Check(book.levels.Length==100,"100 levels");
        var fingerprints=new HashSet<string>();
        foreach(var c in book.levels)
        {
            var l=LevelFactory.Create(c);
            Check(l.socks.Count==c.pairs*2+c.odd,"Count "+c.level);
            Check(l.socks.Where(s=>!s.odd).GroupBy(s=>s.Key).All(g=>g.Count()%2==0),"Pairs balanced "+c.level);
            Check(l.socks.All(s=>s.depth<c.layers),"Layer budget "+c.level);
            Check(l.socks.Count(s=>s.line)==c.line*2,"Line count "+c.level);
            Check(l.socks.Count(s=>s.washer)==c.washer*2,"Washer count "+c.level);
            Check(l.socks.Count(s=>s.wet)==c.wet*2,"Wet count "+c.level);
            Check(l.socks.Count(s=>s.knot)==c.knots*2,"Knot count "+c.level);
            Check(Puzzle.Validate(c)==null,Puzzle.Validate(c)??"solved");
            string fp=string.Join(";",l.socks.Select(s=>$"{s.Key},{s.slot},{s.depth},{s.release}"));
            Check(fingerprints.Add(fp),"Unique layout "+c.level);
            Check(fp==string.Join(";",LevelFactory.Create(c).socks.Select(s=>$"{s.Key},{s.slot},{s.depth},{s.release}")),"Reproducibility "+c.level);
            // Test many alternative seeds, not merely the checked-in witness.
            int seed=c.seed;
            for(int i=1;i<=20;i++){c.seed=seed+i*37;Check(Puzzle.Validate(c)==null,"Seed variant "+c.level+":"+i);}
            c.seed=seed;
            var randomPlay=new Puzzle(LevelFactory.Create(c));
            var rng=new Random(c.seed);
            for(int step=0;step<35&&!randomPlay.lost&&!randomPlay.Won;step++)
            {
                var legal=randomPlay.Socks.Where(s=>s.place!=Place.Selected&&randomPlay.Available(s)).ToArray();
                if(legal.Length==0)break;
                randomPlay.Tap(legal[rng.Next(legal.Length)].id);
            }
            randomPlay.Reshuffle(c.seed);
            foreach(int id in randomPlay.layout.solution.ToArray())
            {
                if(randomPlay.Socks[id].place==Place.Gone||randomPlay.selected==id)continue;
                var action=randomPlay.Tap(id);if(action==TapResult.Untied)action=randomPlay.Tap(id);
                Check(action!=TapResult.Lost&&action!=TapResult.Blocked,"Reshuffle recovery "+c.level);
            }
            Check(randomPlay.Won,"Reshuffle completion "+c.level);
        }
        var config=new LevelConfig{pairs=4,designs=4,layers=1,basket=1,seed=1};
        var p=new Puzzle(LevelFactory.Create(config));
        Check(p.Tap(0)==TapResult.Selected,"Select");
        Check(p.Tap(2)==TapResult.Selected && p.basket.Count==1,"Stash");
        Check(p.Tap(4)==TapResult.Lost,"Overflow");
        p.AddSpot();Check(!p.lost && p.capacity==2,"Continue");
        Check(p.Tap(4)==TapResult.Selected,"Continue move");
        Check(p.Tap(5)==TapResult.Matched,"Match");
        Check(p.Tap(0)==TapResult.Selected && p.basket.Count==1,"Basket select");
        Check(p.Tap(1)==TapResult.Matched,"Basket match");
        CheckOpening(book);
        Directory.CreateDirectory("Documentation/Opening-Progression");
        File.WriteAllText("Documentation/Opening-Progression/layout-fixtures.json",JsonSerializer.Serialize(book.levels.Take(20).Select(c=>new {config=c,solution=LevelFactory.Create(c).solution}),new JsonSerializerOptions{IncludeFields=true,WriteIndented=true}));
        Console.WriteLine($"PASS: {assertions} assertions; 100 unique levels; 2,100 complete solution simulations.");
    }
    static void CheckOpening(LevelBook book)
    {
        Check(book.levels.Take(2).All(c=>c.pairs<=4&&c.layers==1&&c.basket==0),"First two levels remain short, unrestricted introductions");
        Check(book.levels.Take(20).All(c=>c.odd+c.washer+c.line+c.knots+c.wet+c.sizes==0),"Opening teaches layers and basket without extra modifiers");
        Check(book.levels.Select(c=>c.level).Where(n=>book.levels[n-1].HasTutorial).SequenceEqual(new[]{1,3,4,21,31,41,51,61,71,81,91}),"Tutorials introduce mechanics at the correct levels, without repeating basket at 11");
        var reveal=new Puzzle(LevelFactory.Create(book.levels[2]));
        Check(reveal.Tap(2)==TapResult.Blocked&&reveal.moves==0,"Level 3 blocks the covered sock");
        Check(reveal.Tap(0)==TapResult.Selected&&reveal.Tap(1)==TapResult.Matched,"Level 3 top pair matches");
        Check(reveal.Available(reveal.Socks[2])&&reveal.Available(reveal.Socks[3]),"Clearing the pair reveals another complete pair");
        foreach(var c in book.levels.Take(10))
        {
            bool needsHolding=new[]{4,5,7,8,10}.Contains(c.level);
            Check(CanFinish(c,0)!=needsHolding,"Independent search confirms intended basket decision at level "+c.level);
            Check(CanFinish(c,1),"Opening can finish with at most one holding spot at level "+c.level);
            if(c.level>=4)Check(c.basket==5,"Generous five-spot basket at level "+c.level);
            var hintPlay=new Puzzle(LevelFactory.Create(c));
            for(int step=0;step<100&&!hintPlay.Won;step++)
            {
                int id=hintPlay.Hint();Check(id>=0,"Hint exists for opening "+c.level);
                var action=hintPlay.Tap(id);Check(action!=TapResult.Blocked&&action!=TapResult.Lost,"Hint is legal for opening "+c.level);
            }
            Check(hintPlay.Won,"Hints can complete opening "+c.level);
        }
        Check(book.levels.Take(20).All(c=>c.revision==1)&&book.levels.Skip(20).All(c=>c.revision==0),"Only changed layouts require active-save migration");
    }
    // Explore actual legal moves independently of the stored solution witness.
    static bool CanFinish(LevelConfig config,int peakLimit)
    {
        var queue=new Queue<Puzzle>();queue.Enqueue(new Puzzle(LevelFactory.Create(config)));
        var seen=new HashSet<string>();
        while(queue.Count>0)
        {
            var p=queue.Dequeue();if(p.Won)return true;
            string state=p.selected+":"+string.Join(",",p.Socks.Select(s=>(int)s.place));if(!seen.Add(state))continue;
            foreach(var sock in p.Socks.Where(s=>s.place!=Place.Selected&&p.Available(s)))
            {
                var next=new Puzzle(LevelFactory.Create(config)){selected=p.selected,matched=p.matched};
                for(int i=0;i<p.Socks.Count;i++)next.Socks[i].place=p.Socks[i].place;
                next.basket.AddRange(p.basket);
                var action=next.Tap(sock.id);
                if(action!=TapResult.Blocked&&action!=TapResult.Lost&&next.basket.Count<=peakLimit)queue.Enqueue(next);
            }
        }
        return false;
    }
}
