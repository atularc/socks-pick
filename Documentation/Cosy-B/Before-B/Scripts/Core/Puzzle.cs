using System;
using System.Collections.Generic;
using System.Linq;

namespace SockPairs.Core
{
    [Serializable] public class LevelBook { public LevelConfig[] levels; }
    [Serializable] public class LevelConfig
    {
        public int level, chapter, difficulty, pairs, designs, layers, basket, lookalikes, odd,
            washer, line, knots, wet, sizes, seconds, coins, botTarget, seed;
        public string type;
    }
    public enum Place { Pile, Selected, Basket, Gone }
    public enum TapResult { Blocked, Selected, Matched, Binned, Untied, Lost }
    [Serializable] public class Sock
    {
        public int id, pair, design, size, slot, depth, release;
        public bool odd, knot, line, washer, wet;
        public Place place;
        public int Key => design * 3 + size;
    }
    public class Layout
    {
        public LevelConfig config;
        public List<Sock> socks = new List<Sock>();
        public List<int> solution = new List<int>();
    }
    public static class LevelFactory
    {
        // A seeded legal tap sequence is constructed first. Stack dependencies are then
        // built from that sequence, so every generated layout carries a solution witness.
        public static Layout Create(LevelConfig c)
        {
            var rng = new Random(c.seed);
            var result = new Layout { config = c };
            var pairOrder = Enumerable.Range(0, c.pairs).ToArray();
            // Keep the first three pairs free of progress gates.
            var special = Enumerable.Range(Math.Min(3, c.pairs), Math.Max(0, c.pairs - 3)).ToList();
            Shuffle(special, rng);
            var washer = Take(special, c.washer);
            var line = Take(special, c.line);
            // Other modifiers may overlap location types; release still follows the witness.
            var modifiers = Enumerable.Range(Math.Min(3, c.pairs), Math.Max(0, c.pairs - 3)).ToList();
            Shuffle(modifiers, rng);
            var wet = Take(modifiers, c.wet);
            var knots = Take(modifiers, c.knots);
            var sizes = Take(modifiers, c.sizes);
            int[] palette={0,7,2,9,4,11,6,1,8,3,10,5,12,19,14,21,16,23,18,13,20,15,22,17};
            var designs = palette.Take(Math.Max(1,c.designs)).ToList();
            Shuffle(designs, rng);
            for (int p = 0; p < c.pairs; p++)
            {
                int design = designs[p % designs.Count];
                int size = sizes.Contains(p) ? (p % 2 == 0 ? 0 : 2) : 1;
                for (int side = 0; side < 2; side++) result.socks.Add(new Sock {
                    id = p * 2 + side, pair = p, design = design, size = size,
                    washer = washer.Contains(p), line = line.Contains(p), wet = wet.Contains(p),
                    knot = knots.Contains(p), release = washer.Contains(p) ? 3 : wet.Contains(p) ? 2 : 0
                });
            }
            // Teach the unrestricted first three matches, then increase the number of
            // simultaneously open pairs. The basket always has at least one spare spot.
            int intro = Math.Min(3, c.pairs);
            for (int p = 0; p < intro; p++) { result.solution.Add(p * 2); result.solution.Add(p * 2 + 1); }
            var open = new List<int>();
            int next = intro;
            int pressure = c.basket == 0 ? 1 : Math.Min(c.basket, 2 + (c.level - 11) / 22);
            while (next < c.pairs || open.Count > 0)
            {
                bool start = next < c.pairs && !open.Any(p => result.socks[p * 2].Key == result.socks[next * 2].Key)
                    && (open.Count == 0 || open.Count < pressure && rng.NextDouble() < 0.72);
                if (start) { result.solution.Add(next * 2); open.Add(next++); }
                else { int i = rng.Next(open.Count); result.solution.Add(open[i] * 2); result.solution.Add(open[i] * 2 + 1); open.RemoveAt(i); }
            }
            for (int i = 0; i < c.odd; i++)
            {
                int id = result.socks.Count;
                result.socks.Add(new Sock { id = id, pair = -1, design = i % Math.Max(1, c.designs), size = 1, odd = true });
                result.solution.Insert(rng.Next(result.solution.Count + 1), id);
            }
            int[] heights = new int[36];
            int normalCount=result.socks.Count(s=>!s.line);
            int activeCount=Math.Min(30,(int)Math.Ceiling((double)normalCount/Math.Max(1,c.layers)));
            var normalSlots=Enumerable.Range(0,30).ToList();
            if(c.layers==1&&c.pairs<=8)normalSlots=Enumerable.Range(0,normalCount).ToList();
            else Shuffle(normalSlots,rng);
            normalSlots=normalSlots.Take(activeCount).ToList();
            foreach (int id in result.solution.Distinct())
            {
                var s = result.socks[id];
                int from = s.line ? 30 : 0, to = s.line ? 36 : 30;
                var available = (s.line?Enumerable.Range(from,to-from):normalSlots).Where(x => heights[x] < c.layers).ToList();
                if (available.Count == 0) throw new InvalidOperationException("Too many socks for the layer budget: " + c.level);
                // Prefer filling a layer before making the next one, while randomizing positions.
                int lowest = available.Min(x => heights[x]);
                var candidates = available.Where(x => heights[x] == lowest).ToList();
                int slot = candidates[rng.Next(candidates.Count)];
                s.slot = slot; s.depth = heights[slot]++;
            }
            return result;
        }
        static HashSet<int> Take(List<int> values, int count)
        {
            count = Math.Min(values.Count, count);
            var result = new HashSet<int>(values.Take(count)); values.RemoveRange(0, count); return result;
        }
        static void Shuffle<T>(IList<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--) { int j = rng.Next(i + 1); T t = list[i]; list[i] = list[j]; list[j] = t; }
        }
    }
    public class Puzzle
    {
        public readonly Layout layout;
        public List<Sock> Socks => layout.socks;
        public readonly List<int> basket = new List<int>();
        public int selected = -1, matched, capacity, moves;
        public bool lost;
        public bool Won => Socks.All(s => s.place == Place.Gone);
        public Puzzle(Layout layout) { this.layout = layout; capacity = layout.config.basket; }
        public bool Exposed(Sock s) => s.place != Place.Gone && (s.place != Place.Pile || !Socks.Any(t => t.slot == s.slot && t.depth < s.depth && t.place == Place.Pile));
        public bool Available(Sock s) => Exposed(s) && (s.place != Place.Pile || matched >= s.release);
        public TapResult Tap(int id)
        {
            if (id < 0 || id >= Socks.Count || lost || Won) return TapResult.Blocked;
            var s = Socks[id];
            if (!Available(s) || s.place == Place.Selected) return TapResult.Blocked;
            if (s.knot)
            {
                foreach (var partner in Socks.Where(t => t.pair == s.pair && !t.odd)) partner.knot = false;
                moves++; return TapResult.Untied;
            }
            if (s.odd) { s.place = Place.Gone; moves++; return TapResult.Binned; }
            if (selected >= 0 && Socks[selected].Key == s.Key)
            {
                basket.Remove(id); Socks[selected].place = Place.Gone; s.place = Place.Gone;
                selected = -1; matched++; moves++; return TapResult.Matched;
            }
            bool fromBasket = s.place == Place.Basket;
            if (selected >= 0)
            {
                if (capacity == 0)
                    Socks[selected].place = Place.Pile;
                else
                {
                    if (basket.Count >= capacity && !fromBasket) { lost = true; return TapResult.Lost; }
                    basket.Remove(id); basket.Add(selected); Socks[selected].place = Place.Basket;
                }
            }
            basket.Remove(id); selected = id; s.place = Place.Selected; moves++;
            return TapResult.Selected;
        }
        public void AddSpot() { capacity++; lost = false; }
        public void Reshuffle(int seed)
        {
            var remaining=Socks.Where(s=>s.place!=Place.Gone).OrderBy(s=>s.odd?-1:s.Key).ToList();
            basket.Clear();selected=-1;lost=false;
            var rng=new Random(seed);
            int[] slots=Enumerable.Range(0,30).OrderBy(i=>rng.Next()).ToArray();
            layout.solution.Clear();
            for(int i=0;i<remaining.Count;i++)
            {
                var s=remaining[i];s.place=Place.Pile;s.slot=slots[i%30];s.depth=i/30;s.release=0;s.line=false;s.washer=false;s.wet=false;
                layout.solution.Add(s.id);
            }
        }
        public int Hint()
        {
            // Prefer a match already available, then use the original legal solution order.
            if (selected >= 0)
            {
                int key = Socks[selected].Key;
                var match = Socks.FirstOrDefault(s => s.id != selected && !s.odd && s.Key == key && Available(s));
                if (match != null) return match.id;
                if(capacity>0&&basket.Count>=capacity)
                {
                    foreach(int id in basket)
                        if(Socks.Any(s=>s.id!=id&&!s.odd&&s.Key==Socks[id].Key&&Available(s)))return id;
                    return -1;
                }
            }
            foreach (int id in layout.solution)
            {
                var s = Socks[id];
                if (s.place != Place.Pile || !Available(s)) continue;
                if (s.odd || s.knot) return id;
                if (Socks.Any(t => t.id != id && !t.odd && t.Key == s.Key && Available(t))) return id;
            }
            foreach (int id in layout.solution) if (Socks[id].place == Place.Pile && Available(Socks[id])) return id;
            return -1;
        }
        public static string Validate(LevelConfig c)
        {
            var p = new Puzzle(LevelFactory.Create(c));
            int peak = 0;
            foreach (int id in p.layout.solution)
            {
                if (p.Socks[id].place == Place.Gone || p.selected == id) continue;
                var r = p.Tap(id);
                if (r == TapResult.Untied) r = p.Tap(id);
                if (r == TapResult.Blocked || r == TapResult.Lost) return "Level " + c.level + " failed at sock " + id + ": " + r;
                peak = Math.Max(peak, p.basket.Count);
            }
            return p.Won ? null : "Level " + c.level + " did not clear";
        }
    }
}
