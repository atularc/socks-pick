using System;
using UnityEngine;
using UnityEngine.UI;
namespace SockPairs
{
    public static class ReferenceReward
    {
        const float Scale=850f/380f;
        static Vector2 Position(Rect source)=>new Vector2((source.center.x-1235)*Scale,(501.5f-source.center.y)*Scale);
        static ReferenceBArt Piece(Transform panel,string name,Rect source,Rect destination,float radius=0)
        {var p=Position(destination);return ReferenceBArt.Add(panel,name,source,p.x,p.y,destination.width*Scale,destination.height*Scale,radius);}
        public static RectTransform Build(Transform modal,int amount,int collected,int stars,bool last,Action next)
        {
            var panel=UI.Rect(modal,"Win",0,40,850,605*Scale);
            var plate=ReferenceBArt.Add(panel,"Approved B reward artwork",new Rect(1045,199,380,605),0,0,850,605*Scale);
            plate.rewardOutline=true;
            // Fixed decorative lettering, emblem, rays, studs and coin come directly from B.
            // Only game-dependent values are covered and rendered live.
            Piece(panel,"Coin amount backing",new Rect(1184,512,12,44),new Rect(1195,512,64,44));
            var p=Position(new Rect(1195,510,64,49));
            var count=UI.Label(panel,"+"+amount,p.x,p.y,64*Scale,49*Scale,67,UI.Cream);UI.Stroke(count,UI.Ink,1.4f);
            Piece(panel,"Album count backing",new Rect(1300,594,24,32),new Rect(1305,594,77,32));
            p=Position(new Rect(1305,595,77,29));UI.Label(panel,collected+" / 24",p.x,p.y,77*Scale,29*Scale,40,UI.Ink);
            var track=new Rect(1155,630,225,23);p=Position(track);
            var trackFace=UI.ClayBox(panel,"Album track",p.x,p.y,track.width*Scale,track.height*Scale,UI.Hex("DCA66F")).GetComponent<ClayImage>();trackFace.roundness=.5f;trackFace.depth=2;
            float fillWidth=219*Mathf.Clamp01(collected/24f);
            if(fillWidth>0){p=Position(new Rect(1158,633,fillWidth,17));var fill=UI.ClayBox(panel,"Album fill",p.x,p.y,fillWidth*Scale,17*Scale,UI.Hex("FF9C27")).GetComponent<ClayImage>();fill.roundness=.5f;fill.depth=1;}
            // The reference illustrates three earned stars. Other outcomes retain truthful scoring.
            for(int i=stars;i<3;i++)
            {
                var box=new Rect(1110+i*84,400,84,87);
                Piece(panel,"Unearned star",box,box).UnearnedStar();
            }
            var nextRect=new Rect(1078,686,318,79);
            if(last)
            {
                p=Position(nextRect);UI.Button(panel,"Back to Map",p.x,p.y,nextRect.width*Scale,nextRect.height*Scale,UI.Pink,next,64);
                return panel;
            }
            var face=Piece(panel,"Next level",nextRect,nextRect,25);face.raycastTarget=true;
            var button=face.gameObject.AddComponent<Button>();button.targetGraphic=face;
            button.onClick.AddListener(()=>{SockGame.Instance?.ButtonSound();next();});
            return panel;
        }
    }
}

