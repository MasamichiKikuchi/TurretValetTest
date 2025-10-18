//=============================================================================
// <summary>
// 魔力弾のスポーン地点のクラス
// </summary>
// <author> 菊池雅道 </author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;
using via.physics;
using via.storage.saveService;
using static via.collision;
using static via.dynamics.Vehicle2;
using System.Security;

namespace app
{
    [UpdateOrder((int)AppUpdateOrder_Work.UpdateOrder.Gimmick)]
    public class MagicPowerSpawnPoint_Work : via.Behavior, IColliders
    {
        #region フィールド    
        System.Random random = new System.Random();         //ランダムクラス
        GameFlowManager_Work gameFlowManagerWork = null;    //ゲームフローマネージャー
        private float spawnTimer = 0;                       //魔力玉生成用のタイマー
        private int randomTime = 0;                         //最初の魔力玉生成時間(ランダム)
        private bool contactObject = false;                 //他のものに触れているか
        private bool firstSpawn = true;                     //最初の魔力玉生成を行ったか
        float magicPowerHeghtOffset = 0.5f;                 //魔力玉のスポーン位置高さオフセット値
        float bigMagicPowerHeghtOffset = 1.0f;              //大魔力玉のスポーン位置高さオフセット値
        private float bigMagicPowerScale = 2.5f;            //大魔力弾のスケール
        private string inGameLocationFolderPath = "GameContents/InGame/Location";   //インゲームファイルパス名
        private Folder inGameLocationFolder = null;                                 //インゲームフォルダ
        #endregion

        #region プレハブ
        [DataMember]
        private Prefab magicPowerPrefab = null;             //魔力玉のプレハブ
        #endregion

        #region ユーザーデータ
        [DataMember]
        private MagicPowerUserData_Work magicPowerUserData = null;   //魔力玉に関するユーザーデータ
        #endregion

        //魔力弾の生成
        private void instantiateMagicPower()
        {
            if (magicPowerPrefab == null)
            {
                return;
            }        
            
            magicPowerPrefab.instantiate(GameObject.Transform.Position + new vec3(0, magicPowerHeghtOffset, 0), inGameLocationFolder);           
        }

        //大魔力弾の生成
        private void instantiateBigMagicPower()
        {
            if (magicPowerPrefab == null)
            {
                return;
            }
        
            GameObject magicaPower = magicPowerPrefab.instantiate(GameObject.Transform.Position + new vec3(0, bigMagicPowerHeghtOffset, 0), inGameLocationFolder);
            magicaPower.getComponent<Transform>().LocalScale *= bigMagicPowerScale;
            magicaPower.Tag = "BigMagicPower";         
        }

        //魔力玉スポーンのプロセス
        public void spawnMagicPower()
        {
            //タイマーリセット
            spawnTimer = 0.0f;

            //確率で大魔力玉を生成
            System.Random bigMagicPowerRandom = new System.Random();
            int bigMagicPowerProbability = 0;
            //時間に応じて確立を変化
            if (gameFlowManagerWork.IngameTimer <= magicPowerUserData.BigMagicPowerProbabilityChangeTime)
            {
                bigMagicPowerProbability = 100 / magicPowerUserData.LastBigMagicPowerProbability;
            }
            else 
            {
                bigMagicPowerProbability = 100 / magicPowerUserData.FirstBigMagicPowerProbability;
            }
            
            int bigMagicPowerNum = bigMagicPowerRandom.Next(0, bigMagicPowerProbability);

            if (bigMagicPowerNum == 0)
            {
                //大魔力弾を生成
                instantiateBigMagicPower();
            }
            else
            {
                //魔力弾を生成
                instantiateMagicPower();
            }
        }

		public override void start()
		{
            inGameLocationFolder = SceneManager.CurrentScene.findFolder(inGameLocationFolderPath);
            gameFlowManagerWork = SceneManager.MainScene.findGameObject("GameSystem").getComponent<GameFlowManager_Work>();

            //最初のスポーン時間を範囲内からランダムに設定する
            //設定範囲内の秒数を１秒ずつ配列に収納
            int timeElementNum = magicPowerUserData.MagicPowerRandomTimeMax - magicPowerUserData.MagicPowerRandomTimeMin;
            if (timeElementNum <= 0)
            {
                timeElementNum = 1;
            }
            int[] array = new int[timeElementNum];
            for (int i = 0; i < timeElementNum; i++)
            {
                array[i] = i + magicPowerUserData.MagicPowerRandomTimeMin;
            }

            //秒数の配列をシャッフルし、その中から1つ選んでスポーン時間として設定する
            int j;
            int k;
            int tmp;       
            j = timeElementNum - 1;
            while (j > 0)
            {
                k = random.Next(j + 1);

                tmp = array[k];

                array[k] = array[j];

                array[j] = tmp;

                j--;
            }

            //スポーン時間を設定
            randomTime = array[random.Next(j)];
        }

		public override void update()
		{
            //ゲーム開始していなければ、処理をしない
            if (gameFlowManagerWork.GameStart == false)
            {
                return;
            }

            //他のオブジェクトに接触している場合、処理をしない
            if (contactObject == true)
            {
                return;
            }

            //スポーン時間
            spawnTimer += via.Application.ElapsedSecond;

            //最初とそれ以外で魔力弾スポーンの時間を変える
            switch (firstSpawn)
            { 
                case true:
                    //最初のスポーンはランダム時間
                    if (Math.Truncate(spawnTimer) >= randomTime)
                    {                        
                        spawnMagicPower();

                        //最初のスポーンフラグを無効
                        firstSpawn = false;
                    }

                    break;

                case false:

                    //最初以外のスポーンは固定時間
                    if (Math.Truncate(spawnTimer) >= magicPowerUserData.MagicPowerSpawnIntervalTime)
                    {                       
                        spawnMagicPower();
                    }

                    break;           
            }
                  
        }       

        /// <summary>
        /// 衝突した
        /// </summary>
        /// <param name="info"></param>
        public void onContact(CollisionInfo collision_info)
        {
            //接触フラグ有効
            contactObject = true;
        }

        void IColliders.onOverlapping(CollisionInfo collision_info)
        {
            //接触フラグ有効
            contactObject = true;
        }

        void IColliders.onSeparate(CollisionInfo collision_info)
        {
            //接触フラグ無効
            contactObject = false;
        }
    }
}
