//=============================================================================
// <summary>
// ゲームシーン上に一つだけ存在させたいクラスに継承させる 
// </summary>
// <author> 菊池雅道 </author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;

namespace app
{
    public abstract class SingletonRoot_Work<T> : via.Behavior
    where T : Behavior
    {
        /// <summary>
        /// 直接objectを使いたいときのクラス
        /// </summary>
        public class ReLibObject
        {

        }

        /// <summary>
        /// ロック用のobject
        /// 循環参照を避けるために使う
        /// </summary>
        public class ReLibLockObject : ReLibObject
        {
        }

        #region プロパティ

        public static T Instance
        {
            get
            {
#if VIA_DEVELOP
                //編集用のインスタンスを返す
                if (SceneManager.CurrentScene.Construct)
                {
                    T inst;

                    //編集中のものは無い場合は生成する
                    createInstanceOnDevelop();

                    //この間に_InstanceEditが書き換えられる可能性があるのでロックして結果を保持
                    lock (LockObj)
                    {
                        inst = _InstanceEdit;
                    }
                    return inst;
                }
                else
                {
                    if (_Instance == null)
                    {
                        debug.errorLine("{0}インスタンスが生成されていません。{0}コンポーネントを使用したGameObjectを配置してください。", typeof(T).Name);
                    }
                    return _Instance;
                }
#else
                return _Instance;
#endif
            }
        }

        #endregion

        #region フィールド
        private static T _Instance = null;
        static ReLibLockObject LockObj = new ReLibLockObject();

#if VIA_DEVELOP
        private static T _InstanceEdit = null;

        /// <summary>
        /// 編集用のマネージャか否か
        /// </summary>
        protected bool IsEdit = false;
#endif
        #endregion

        #region 基本メソッド
        /// <summary>
        /// シーンにロード
        /// </summary>
        public override void onLoad()
        {
            Initialize(this as T);
        }

        /// <summary>
        /// シーンから破棄
        /// </summary>
        public override void onDestroy()
        {
            //インスタンスを削除
            clearInstance();
        }
        #endregion


        #region 非公開メソッド
        /// <summary>
        /// インスタンスを設定
        /// </summary>
        /// <param name="instance"></param>
        protected static bool Initialize(T instance)
        {
            bool is_create = false;

            lock (LockObj)
            {
#if VIA_DEVELOP
                if (SceneManager.CurrentScene.Construct == false)
                {
                    //再生用
                    if (_Instance == null)
                    {
                        _Instance = instance;
                        is_create = true;
                    }
                }
                else
                {
                    //編集用
                    if (_InstanceEdit == null)
                    {
                        _InstanceEdit = instance;
                        is_create = true;

                        //編集用とする
                        var inst = instance as SingletonRoot_Work<T>;
                        inst.IsEdit = true;
                    }
                }
#else
                    if (_Instance == null)
                    {
                        if (SceneManager.CurrentScene.Construct == false)
                        {
                            _Instance = instance;
                            is_create = true;

                        }                      
                    }
#endif
            }

            return is_create;
        }

        /// <summary>
        /// インスタンスをクリア
        /// </summary>
        public static void clearInstance()
        {
            lock (LockObj)
            {
#if VIA_DEVELOP
                if (SceneManager.CurrentScene.Construct == false)
                {
                    //再生用
                    if (_Instance != null)
                    {
                        _Instance = null;
                    }
                }
                else
                {
                    //編集用
                    if (_InstanceEdit != null)
                    {
                        _InstanceEdit = null;
                    }
                }
#else
                    if (_Instance != null)
                    {
                        _Instance = null;
                    }
#endif
            }
        }

#if VIA_DEVELOP
        /// <summary>
        /// 編集用のインスタンスを生成する
        /// </summary>
        static void createInstanceOnDevelop()
        {
            //並列で呼ばれることがあるのでロックする
            lock (LockObj)
            {
                //シーン編集中は
                //再生後にstaticのフィールドが上書きされるためインスタンスがnullになるので
                //編集用のSingletonは使用時に再生成する
                if (SceneManager.CurrentScene.Construct)
                {
                    if (_InstanceEdit == null)
                    {

                        foreach (var trans in SceneManager.CurrentScene.Children)
                        {
                            var comp = trans.GameObject.getComponent<T>();
                            if (comp != null)
                            {
                                _InstanceEdit = comp;
                            }
                        }

                        if (_InstanceEdit == null)
                        {
                            //新しい編集用のEffectManagerを生成する
                            var go = GameObject.create("EffectManagerForEdit");
                            var create_inst = go.createComponent<T>();
                            _InstanceEdit = create_inst;
                        }

                        //編集用
                        var instance = _InstanceEdit as SingletonRoot_Work<T>;
                        instance.IsEdit = true;
                    }

                }

            }
        }
#endif
        #endregion

    }
}
