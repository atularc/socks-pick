using UnityEngine;

namespace SockPairs
{
    public class SoundBank : MonoBehaviour
    {
        AudioSource fx,music;
        AudioClip tap,match,drop,win,warning;
        public void Initialize()
        {
            fx=gameObject.AddComponent<AudioSource>();fx.playOnAwake=false;fx.mute=true;
            music=gameObject.AddComponent<AudioSource>();music.playOnAwake=false;music.loop=true;music.volume=.065f;music.mute=true;
            tap=Tone("lift",440,.08f);match=Tone("pair",740,.24f);drop=Tone("basket",190,.13f);
            win=Tone("tidy",880,.65f);warning=Tone("oops",130,.22f);
            music.clip=Music();music.Play();
        }
        static AudioClip Tone(string name,float hz,float length)
        {
            const int rate=22050;var samples=new float[(int)(rate*length)];
            for(int i=0;i<samples.Length;i++)
            {
                float t=(float)i/rate,e=Mathf.Sin(Mathf.PI*i/samples.Length)*Mathf.Exp(-4*t/length);
                samples[i]=.35f*e*(Mathf.Sin(2*Mathf.PI*hz*t)+.22f*Mathf.Sin(2*Mathf.PI*hz*2*t));
            }
            var clip=AudioClip.Create(name,samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
        }
        static AudioClip Music()
        {
            const int rate=22050;int length=rate*16;var samples=new float[length];
            float[] notes={261.63f,329.63f,392,329.63f,293.66f,349.23f,440,349.23f,261.63f,329.63f,392,523.25f,293.66f,349.23f,392,329.63f};
            for(int n=0;n<notes.Length;n++)for(int j=0;j<rate;j++)
            {float t=(float)j/rate; samples[n*rate+j]=Mathf.Sin(2*Mathf.PI*notes[n]*t)*Mathf.Sin(Mathf.PI*t)*Mathf.Exp(-4*t)*.3f;}
            var c=AudioClip.Create("Laundry lullaby",length,1,rate,false);c.SetData(samples,0);return c;
        }
        public void Settings(bool sounds,bool musicOn,bool hostAudio)
        { if(!fx)return;fx.mute=!sounds||!hostAudio;music.mute=!musicOn||!hostAudio; }
        public void Play(string cue)
        { if(!fx)return;fx.PlayOneShot(cue=="match"?match:cue=="win"?win:cue=="drop"?drop:cue=="lose"?warning:tap,.65f); }
    }
}
