//=============================================================================
// <summary>
// 魔力弾スポーン位置の生成に関するクラス
// </summary>
// <author>菊池雅道</author>
//=============================================================================
using System;
using System.Collections.Generic;
using System.Reflection;
using via;
using via.attribute;

namespace app
{
	public class MagicPower : via.Behavior
	{
        #region フィールド
        [DataMember]
        private List<Position> spawnPositionList = new List<Position>();            //魔力玉スポーン位置リスト
        private List<Prefab> magicPowerSpawnpoint = new List<Prefab>();             //魔力玉スポーン位置プレハブリスト
        private string inGameLocationFolderPath = "GameContents/InGame/Location";   //インゲームフォルダパス
        private Folder inGameLocationFolder = null;                                 //インゲームフォルダ
        #endregion
        
        #region プレハブ
        [DataMember]
        private Prefab magicPowerSpawnPointPrefab = null;                           //魔力玉スポーン位置プレハブ
        #endregion

        public override void awake()
		{
            //フォルダ取得
            inGameLocationFolder = SceneManager.CurrentScene.findFolder(inGameLocationFolderPath);

            //魔力玉スポーン位置を設置
            foreach (var i in spawnPositionList)
            {
                var magicPowerSpawnPoint = magicPowerSpawnPointPrefab.instantiate(i, inGameLocationFolder);
                magicPowerSpawnPoint.Tag = "Pausable";
            }
        }
    }
}
