using System.Collections.Generic;
using System.Threading;
using System.IO;
using System;
using TrueGearSDK;
using TrueGear;

namespace MyTrueGear
{
    public class TrueGearMod
    {
        private static TrueGearPlayer _player = null;

        private static ManualResetEvent minddeathwaveMRE = new ManualResetEvent(false);

        public void MindDeathWave()
        {
            minddeathwaveMRE.WaitOne();
            _player.SendPlay("MindDeathWave");
            Thread.Sleep(100);
        }

        public TrueGearMod() 
        {

            _player = new TrueGearPlayer();
            //_player = new TrueGearPlayer("617830","SuperHotVR");
            //_player.PreSeekEffect("PickUpItemRight");
            RegisterFilesFromDisk();
            _player.Start();

            new Thread(new ThreadStart(this.MindDeathWave)).Start();
        }

        private void RegisterFilesFromDisk()
        {
            FileInfo[] files = new DirectoryInfo(".//Mods//TrueGear")   //  (".//BepInEx//plugins//TrueGear")
                    .GetFiles("*.asset_json", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                string name = files[i].Name;
                string fullName = files[i].FullName;
                if (name == "." || name == "..")
                {
                    continue;
                }
                string jsonStr = File.ReadAllText(fullName);
                JSONNode jSONNode = JSON.Parse(jsonStr);
                EffectObject _curAssetObj = EffectObject.ToObject(jSONNode.AsObject);
                string uuidName = Path.GetFileNameWithoutExtension(fullName);
                _curAssetObj.uuid = uuidName;
                _curAssetObj.name = uuidName;
                _player.SetupRegister(uuidName, jsonStr);
            }
        }

        public void Play(string Event)
        { 
            _player.SendPlay(Event);
        }

        public void StartMindDeathWave()
        {
            minddeathwaveMRE.Set();
        }

        public void StopMindDeathWave()
        {
            minddeathwaveMRE.Reset();
        }

    }
}
