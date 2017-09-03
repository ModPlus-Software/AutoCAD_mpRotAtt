﻿#if ac2010
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
#elif ac2013
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
#endif
using System;
using mpMsg;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace mpRotAtt
{
    public class MpRotAtt
    {
        [CommandMethod("ModPlus", "mpRotAtt", CommandFlags.UsePickSet)]
        public static void Main()
        {
            try
            {
                var ed = AcApp.DocumentManager.MdiActiveDocument.Editor;
                var db = AcApp.DocumentManager.MdiActiveDocument.Database;

                var filList = new[] {new TypedValue((int) DxfCode.Start, "INSERT")};
                var filter = new SelectionFilter(filList);
                var opts = new PromptSelectionOptions
                {
                    MessageForAdding = "\n" + "Выберите блоки: "
                };
                var res = ed.GetSelection(opts, filter);
                if (res.Status != PromptStatus.OK)
                    return;

                var pdo = new PromptDoubleOptions("\n" + "Новый угол поворота атрибутов: ") {DefaultValue = 0};
                var pdr = ed.GetDouble(pdo);
                var ang = pdr.Value;
                if (pdr.Status == PromptStatus.OK)
                {
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        // Start the transaction
                        var selSet = res.Value;
                        var idArray = selSet.GetObjectIds();
                        foreach (var blkId in idArray)
                        {
                            var blkRef = (BlockReference) tr.GetObject(blkId, OpenMode.ForRead);
                            var btr = (BlockTableRecord) tr.GetObject(blkRef.BlockTableRecord, OpenMode.ForRead);
                            btr.Dispose();
                            var attCol = blkRef.AttributeCollection;
                            foreach (ObjectId attId in attCol)
                            {
                                var attRef = (AttributeReference) tr.GetObject(attId, OpenMode.ForWrite);
                                // Переводим в радианы
                                attRef.Rotation = ang * Math.PI / 180.0;
                            }
                        }
                        tr.Commit();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MpExWin.Show(ex);
            }
        }
    }
}