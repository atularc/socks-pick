mergeInto(LibraryManager.library, {
  SP_Init: function(namePtr) {
    var name = UTF8ToString(namePtr);
    window.sockBridge = { name: name, loaded: false, pending: null, saving: false };
    var send = function(method, value) { SendMessage(name, method, value); };
    var host = typeof ytgame !== 'undefined' && ytgame.IN_PLAYABLES_ENV;
    window.sockBridge.host = host;
    send('SetYouTube', host ? '1' : '0');
    send('SetRewardedAdsAvailable', host && ytgame.ads && typeof ytgame.ads.requestRewardedAd === 'function' ? '1' : '0');
    if (host) {
      var audio = function() { send('HostAudio', ytgame.system.isAudioEnabled() ? '1' : '0'); };
      audio(); ytgame.system.onAudioEnabledChange(audio);
      ytgame.system.onPause(function() {
        send('HostPause', '1');
        if (typeof _JS_Sound_SetPaused === 'function') _JS_Sound_SetPaused(1);
        if (typeof Browser !== 'undefined') Browser.mainLoop.pause();
      });
      ytgame.system.onResume(function() {
        if (typeof Browser !== 'undefined') Browser.mainLoop.resume();
        send('HostPause', '0'); audio();
        if (typeof _JS_Sound_SetPaused === 'function') _JS_Sound_SetPaused(0);
      });
      var load = function() {
        ytgame.game.loadData().then(function(data) {
          window.sockBridge.loaded = true;
          send('SaveLoaded', typeof data === 'string' ? data : '');
        }).catch(function(error) {
          send('SaveError', String(error));
          var overlay=document.getElementById('loading');
          if(overlay){overlay.style.display='flex';overlay.querySelector('p').textContent='Could not load your progress. Tap to retry.';overlay.onclick=function(){overlay.onclick=null;load();};}
        });
      };
      load();
    } else {
      var data = '';
      try { data = localStorage.getItem('sock-pairs-save-v1') || ''; } catch(e) { send('SaveError','Browser storage unavailable; progress lasts for this session.'); }
      window.sockBridge.loaded = true; send('SaveLoaded', data);
      document.addEventListener('visibilitychange', function() { send('HostPause', document.hidden ? '1' : '0'); });
    }
  },
  SP_Save: function(jsonPtr) {
    var json = UTF8ToString(jsonPtr), bridge = window.sockBridge;
    if (!bridge || !bridge.loaded) return;
    if (!bridge.host) {
      try { localStorage.setItem('sock-pairs-save-v1', json); } catch(e) { SendMessage(bridge.name,'SaveError',String(e)); }
      return;
    }
    // Serialize writes and coalesce rapid taps so old saves cannot overwrite new ones.
    bridge.pending = json;
    var flush = function() {
      if(bridge.saving || bridge.pending === null) return;
      var data=bridge.pending;bridge.pending=null;bridge.saving=true;
      ytgame.game.saveData(data).catch(function(e) { SendMessage(bridge.name,'SaveError',String(e)); })
        .finally(function() { bridge.saving=false;flush(); });
    };
    flush();
  },
  SP_Ready: function() {
    var overlay=document.getElementById('loading');if(overlay)overlay.style.display='none';
    if(window.sockBridge && window.sockBridge.host) ytgame.game.gameReady();
  },
  SP_Reward: function(rewardPtr) {
    var reward=UTF8ToString(rewardPtr),bridge=window.sockBridge;
    if(!bridge || !bridge.host || !ytgame.ads || !ytgame.ads.requestRewardedAd){if(bridge)SendMessage(bridge.name,'RewardResult','0');return;}
    Promise.resolve().then(function(){return ytgame.ads.requestRewardedAd(reward);}).then(function(earned){SendMessage(bridge.name,'RewardResult',earned === true?'1':'0');})
      .catch(function(){SendMessage(bridge.name,'RewardResult','0');});
  }
});
