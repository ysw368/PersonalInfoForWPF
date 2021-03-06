﻿using DataAccessLayer;
using InterfaceLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

namespace DetailTextNode
{
    public class DetailTextAccess:IDataAccess
    {
        private String _conn = "";
        public String EFConnectionString
        {
            get
            {
                return _conn;
            }
            
            set
            {
                _conn = value;
                
            }
        }
       
        public DetailTextAccess(String EFConnectionString)
        {
            this.EFConnectionString = EFConnectionString;
            repository = new DetailTextRepository(EFConnectionString);
        }

        private DetailTextRepository repository = null;

        public int Create(IDataInfo dataInfoObject)
        {
            if (dataInfoObject == null || (dataInfoObject as DetailTextInfo)==null)
            {
                return 0;
            }
            DetailTextDB dbobj = DetailTextHelper.changeToDetailTextDB(dataInfoObject as DetailTextInfo);
            int result=repository.Create(dbobj);
            //将数据库生成的ID值传回
            dataInfoObject.ID = dbobj.ID;
            return result;
        }
        /// <summary>
        /// 删除本节点及下属所有子节点的记录
        /// </summary>
        /// <param name="nodePath"></param>
        /// <returns></returns>
        public int DeleteDataInfoObjectOfNodeAndItsChildren(String nodePath)
        {
          
            if (String.IsNullOrEmpty(nodePath))
            {
                return 0;
            }
            return repository.DeleteNodeAndItsChild(nodePath);
        }
       
        /// <summary>
        /// 将对象的当前值写入数据库
        /// </summary>
        /// <param name="dataInfoObject"></param>
        /// <returns></returns>
        public int UpdateDataInfoObject(IDataInfo dataInfoObject)
        {
            DetailTextInfo obj=dataInfoObject as DetailTextInfo;
            if (obj.Text.Length > DALConfig.MaxTextFieldSize)
            {
                obj.Text = obj.Text.Substring(0, DALConfig.MaxTextFieldSize);
            }
            if (dataInfoObject == null || obj == null)
            {
                return 0;
            }
            bool isNew = false;
            DetailTextDB dbobj = repository.GetDataInfoObjectByPath(obj.Path);
            if (dbobj == null)
            {
                dbobj = new DetailTextDB();
                isNew = true;

            }
            if (String.IsNullOrEmpty(dbobj.Text) == false && String.IsNullOrEmpty(obj.Text))
            {
                ShowWarningInfo(String.Format("更新节点{0}时，发现数据库记录中的Text字段不为空，但尝试写入一个空的字串，这有可能是个错误，特此提示。",obj.Path));
            }
            dbobj.ModifyTime = obj.ModifyTime;
            dbobj.Text = obj.Text;
            dbobj.Path = obj.Path;
            dbobj.RTFText = (String.IsNullOrEmpty(obj.RTFText))?null:Encoding.UTF8.GetBytes(obj.RTFText);
            if (isNew)
            {
                return repository.Create(dbobj);
            }
            else
            {
                    return repository.Update(dbobj);
            }
            
        }
        [Conditional("DEBUG")]
        private void ShowWarningInfo(String information)
        {
            MessageBox.Show(information);
        }
        /// <summary>
        /// 根据节点路径提取节点数据，如果未找到，返回null
        /// </summary>
        /// <param name="nodePath"></param>
        /// <returns></returns>
        public IDataInfo GetDataInfoObjectByPath(string nodePath)
        {
            if (String.IsNullOrEmpty(nodePath))
            {
                return null;
            }
            DetailTextDB dbobj = repository.GetDataInfoObjectByPath(nodePath);
            if (dbobj == null)
            {
                return null;
            }
            DetailTextInfo infoObj=DetailTextHelper.changeToDetailTextInfo(dbobj);
            //设置数据己装入标记
            infoObj.HasBeenLoadFromStorage = true;
            return infoObj;

        }

        /// <summary>
        /// 更新节点路径
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        public void UpdateNodePath(String oldPath,String newPath)
        {
            repository.UpdateNodePaths(oldPath, newPath);
        }

        /// <summary>
        /// 删除本节点对象，保留其子节点数据
        /// </summary>
        /// <param name="dataInfoObject"></param>
        /// <returns></returns>
        public int DeleteDataInfoObject(IDataInfo dataInfoObject)
        {
            if (dataInfoObject == null)
            {
                return 0;
            }
            return repository.Delete(dataInfoObject.ID);
        }
    }
}
