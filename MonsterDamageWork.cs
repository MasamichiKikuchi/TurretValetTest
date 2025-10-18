//=============================================================================
// <summary>
// MonsterDamageWork 
// </summary>
// <author> 須永ジン </author>
//=============================================================================
using via;
using via.attribute;
using via.render;


namespace app
{
    public class MonsterDamageWork : via.Behavior
    {

        #region 色変更系
        [DataMember]
        public string MaterialName1;
        [DataMember]
        public string MaterialName2;
        [DataMember]
        public string MaterialName3;
        [DataMember]
        public string MaterialName4;
        [DataMember]
        public string VariableName;

        private uint _MaterialNo1 = 0;
        private uint _MaterialNo2 = 0;
        private uint _MaterialNo3 = 0;
        private uint _MaterialNo4 = 0;
        private uint _VariableNo = 0;

        private Mesh _Mesh;

        #endregion

        [DataMember]
        public int MonsterNo;

        private int HP;
        private int old_HP;
        private int Flushcnt;

        private Monster_Work monster_Work;
        public void OnFlush()
        {
            _Mesh.setMaterialFloat(_MaterialNo1, _VariableNo, 0.0f);
            _Mesh.setMaterialFloat(_MaterialNo2, _VariableNo, 0.0f);
            _Mesh.setMaterialFloat(_MaterialNo3, _VariableNo, 0.0f);
            _Mesh.setMaterialFloat(_MaterialNo4, _VariableNo, 0.0f);
        }
        public void OffFlush()
        {
            _Mesh.setMaterialFloat(_MaterialNo1, _VariableNo, 1.0f);
            _Mesh.setMaterialFloat(_MaterialNo2, _VariableNo, 1.0f);
            _Mesh.setMaterialFloat(_MaterialNo3, _VariableNo, 1.0f);
            _Mesh.setMaterialFloat(_MaterialNo4, _VariableNo, 1.0f);
        }

        public override void start()
        {
            switch (MonsterNo)
            {
                case 1:
                    monster_Work = SceneManager.MainScene.findGameObject("Egi").getComponent<Monster_Work>();
                    break;
                case 2:
                    monster_Work = SceneManager.MainScene.findGameObject("Aari").getComponent<Monster_Work>();
                    break;
            }
            
            old_HP = monster_Work.HitPoint;

            Flushcnt = 0;
            //各コンポーネント取得
            _Mesh = GameObject.getSameComponent<Mesh>();
            var variableNameHash = str.makeHash(VariableName);

            // マテリアルのインデックスとパラメータのインデックスを取得
            var materialNameCount = _Mesh.MaterialNames.Count;

            for (uint materialNo = 0; materialNo < materialNameCount; materialNo++)
            {
                var variableNo = _Mesh.getMaterialVariableIndex(materialNo, variableNameHash);
                if (MaterialName1 == _Mesh.MaterialNames[(int)materialNo])
                {
                    if (variableNo != 0xffu)
                    {   // パラメータが見つかった
                        _MaterialNo1 = materialNo;
                        _VariableNo = variableNo;
                    }
                }
                else if (MaterialName2 == _Mesh.MaterialNames[(int)materialNo])
                {
                    if (variableNo != 0xffu)
                    {   // パラメータが見つかった
                        _MaterialNo2 = materialNo;
                        _VariableNo = variableNo;
                    }
                }
                else if (MaterialName3 == _Mesh.MaterialNames[(int)materialNo])
                {
                    if (variableNo != 0xffu)
                    {   // パラメータが見つかった
                        _MaterialNo3 = materialNo;
                        _VariableNo = variableNo;
                    }
                }
                else if (MaterialName4 == _Mesh.MaterialNames[(int)materialNo])
                {
                    if (variableNo != 0xffu)
                    {   // パラメータが見つかった
                        _MaterialNo4 = materialNo;
                        _VariableNo = variableNo;
                    }
                }

            }
        }
        public override void update()
        {
            HP = monster_Work.HitPoint;
            if (HP != old_HP)
            {
                Flushcnt = 50;
                OnFlush();
            }
            if (Flushcnt > 0)
            {
                Flushcnt--;
            }
            else
            {
                OffFlush();
            }

            old_HP = HP;
        }
    }
}
