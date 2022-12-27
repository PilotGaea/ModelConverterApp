using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using PilotGaea.Serialize;
using PilotGaea.TMPEngine;
using PilotGaea.Geometry;

namespace model_maker
{
    public partial class Form1 : Form
    {
        CModelMaker m_Maker = null;
        Stopwatch m_Stopwatch = new Stopwatch();

        public Form1()
        {
            InitializeComponent();

            //加入功能列表
            List<string> featureNames = new List<string>();
            featureNames.Add("來源KMZ");
            featureNames.Add("輸出OGC I3S");
            featureNames.Add("輸出OGC 3DTiles");
            comboBox_Features.Items.AddRange(featureNames.ToArray());
            comboBox_Features.SelectedIndex = 0;
        }

        private void button_Start_Click(object sender, EventArgs e)
        {
            EnableUI(false);

            //將來源資料輸出成Model圖層
            System.Environment.CurrentDirectory = @"C:\Program Files\PilotGaea\TileMap";//為了順利存取安裝目錄下的相關DLL
            m_Maker = new CModelMaker();
            //設定必要參數
            CModelMaker.MODEL_CREATE_PARAM createParam = new CModelMaker.MODEL_CREATE_PARAM();
            createParam.DbPath = string.Format(@"{0}\..\output\model_maker.DB", Application.StartupPath);
            createParam.LayerName = "test";
            createParam.TerrainDBFile = string.Format(@"{0}\..\data\terrain_maker\terrain.DB", Application.StartupPath);
            createParam.TerrainName = "terrain";
            createParam.ExportType = EXPORT_TYPE.LET_DB;

            MODEL_SRC sourceData = new MODEL_SRC(MODEL_SRC.MESH_SOURCE_TYPE.MESH_SOURCE_KMZ);
            sourceData.FileName = string.Format(@"{0}\..\data\modelset_maker\TC1036955_r1.kmz", Application.StartupPath);

            //監聽轉檔事件
            m_Maker.CreateLayerCompleted += M_Maker_CreateLayerCompleted;
            m_Maker.EndNewEntityCompleted += M_Maker_EndNewEntityCompleted;
            m_Maker.ProgressMessageChanged += M_Maker_ProgressMessageChanged;
            m_Maker.ProgressPercentChanged += M_Maker_ProgressPercentChanged;

            //設定進階參數
            switch (comboBox_Features.SelectedIndex)
            {
                case 0://"基本"
                    break;
                case 1://"輸出OGC I3S"
                    createParam.ExportType = EXPORT_TYPE.LET_OGCI3S;
                    createParam.LayerName = "model_maker_ogci3s";
                    //會在destPath目錄下產生layerName.slpk
                    break;
                case 2://"輸出OGC 3DTiles
                    createParam.ExportType = EXPORT_TYPE.LET_OGC3DTILES;
                    createParam.LayerName = "model_maker_ogc3dtiles";
                    //會在destPath目錄下產生layerName資料夾
                    break;
            }

            m_Stopwatch.Restart();
            //開始非同步轉檔
            bool ret = m_Maker.Create(createParam);
            string message = string.Format("Create{0}", (ret ? "通過" : "失敗"));
            listBox_Main.Items.Add(message);
            ret = m_Maker.NewEntity(sourceData);
            message = string.Format("NewEntity{0}", (ret ? "成功" : "失敗"));
            listBox_Main.Items.Add(message);
            ret = m_Maker.EndNewEntity();
            message = string.Format("EndNewEntity{0}", (ret ? "通過" : "失敗"));
            listBox_Main.Items.Add(message);
        }

        private void M_Maker_CreateLayerCompleted(string LayerName, bool Success, string ErrorMessage)
        {
            string message = string.Format("CreateCompleted{0}", (Success ? "成功" : "失敗"));
            listBox_Main.Items.Add(message);
        }

        private void M_Maker_EndNewEntityCompleted(string LayerName, bool Success, string ErrorMessage)
        {
            m_Stopwatch.Stop();
            string message = string.Format("EndNewEntity{0}", (Success ? "成功" : "失敗"));
            listBox_Main.Items.Add(message);
            message = string.Format("耗時{0}分。", m_Stopwatch.Elapsed.TotalMinutes.ToString("0.00"));
            listBox_Main.Items.Add(message);
        }

        private void M_Maker_ProgressPercentChanged(double Percent)
        {
            progressBar_Main.Value = Convert.ToInt32(Percent);
        }

        private void M_Maker_ProgressMessageChanged(string Message)
        {
            listBox_Main.Items.Add(Message);
        }

        private void EnableUI(bool enable)
        {
            button_Start.Enabled = enable;
            comboBox_Features.Enabled = enable;
        }
    }
}