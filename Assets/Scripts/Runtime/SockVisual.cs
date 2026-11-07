using UnityEngine;
using UnityEngine.UI;
namespace SockPairs
{
    // The editable Canvas template is cloned only for changing puzzle content.
    public sealed class SockVisual:MonoBehaviour
    {
        public Image art,hit,shadow;
        public GameObject knot,hint,selection,hole;
        public Text gate,sizeLabel;
        [System.NonSerialized] public Coroutine movement;
    }
}
