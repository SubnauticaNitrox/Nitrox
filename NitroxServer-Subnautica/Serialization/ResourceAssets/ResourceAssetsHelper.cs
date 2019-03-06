using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using NitroxModel.Logger;
using NitroxServer.UnityStubs;

namespace NitroxServer_Subnautica.Serialization.ResourceAssets
{
    public static class ResourceAssetsHelper
    {
        private static HashSet<ulong> matchingMonoscripts = new HashSet<ulong>();

        public static AssetFileInfoEx FindComponentOfType<T>(AssetsFileInstance assetsFileInstance, ulong gameObjectPathId)
        {
            return FindComponentOfType(typeof(T), assetsFileInstance, gameObjectPathId);
        }

        public static AssetFileInfoEx FindTransform(AssetsFileInstance assetsFileInstance, ulong gameObjectPathId)
        {
            AssetsFileTable assetsTable = assetsFileInstance.table;
            AssetsFile assetsFile = assetsFileInstance.file;
            AssetsFileReader afr = assetsFile.reader;

            ulong prevPos = assetsFile.reader.Position;
            AssetFileInfoEx gameObjectAFI = assetsTable.getAssetInfo(gameObjectPathId);
            if (gameObjectAFI.curFileType != 1)
            {
                Log.Info("An incorrect pathId was passed to FindTransform please check if it points to a proper gameObject: " + gameObjectPathId);
                return null;
            }

            afr.Position = gameObjectAFI.absoluteFilePos;
            afr.Align();

            uint componentCount = afr.ReadUInt32();
            AssetFileInfoEx componentAFI;

            for (int i = 0; i < componentCount; i++)
            {
                uint fileId = (uint)afr.ReadInt32();
                ulong pathId = (ulong)afr.ReadInt64();
                if (fileId == 0)
                {
                    componentAFI = assetsTable.getAssetInfo(pathId);
                    if (componentAFI.curFileType == 4) // Ey look we got it
                    {
                        afr.Position = prevPos;
                        return componentAFI;
                    }
                }
                else
                {
                    AssetsFileInstance dep = assetsFileInstance.dependencies[(int)fileId - 1];
                    componentAFI = dep.table.getAssetInfo(pathId);
                    if (componentAFI.curFileType == 4) // Ey look we got it
                    {
                        afr.Position = prevPos;
                        return componentAFI;
                    }
                }
            }

            Log.Error("Well oddly, a Transform wasnt found or a silent error happened...", new NullReferenceException());

            return null;
        }

        public static AssetFileInfoEx FindComponentOfType(Type type, AssetsFileInstance assetsFileInstance, ulong gameObjectPathId)
        {
            AssetsFileTable assetsTable = assetsFileInstance.table;
            AssetsFile assetsFile = assetsFileInstance.file;
            AssetsFileReader afr = assetsFile.reader;

            ulong prevPos = assetsFile.reader.Position;
            AssetFileInfoEx gameObjectAFI = assetsTable.getAssetInfo(gameObjectPathId);
            if (gameObjectAFI.curFileType != 1)
            {
                Log.Info("An incorrect pathId was passed to FindComponentOfType please check if it points to a proper gameObject: " + gameObjectPathId);
                return null;
            }
            afr.Position = gameObjectAFI.absoluteFilePos;
            afr.Align();

            uint componentCount = afr.ReadUInt32();
            AssetFileInfoEx componentAFI;

            for (int i = 0; i < componentCount; i++)
            {
                uint fileId = (uint)afr.ReadInt32();
                ulong pathId = (uint)afr.ReadInt64();
                if (fileId == 0)
                {
                    componentAFI = assetsTable.getAssetInfo(pathId);
                    if (componentAFI.curFileType == 114)
                    {
                        ulong curPos = afr.Position;
                        afr.Position = componentAFI.absoluteFilePos;
                        afr.Align();
                        afr.Position += 16;

                        int scriptFileId = afr.ReadInt32();
                        long scriptPathId = afr.ReadInt64();

                        if (matchingMonoscripts.Contains((ulong)scriptPathId))
                        {
                            return componentAFI;
                        }

                        AssetsFileInstance monoscriptDep = assetsFileInstance.dependencies[scriptFileId - 1];
                        AssetFileInfoEx monoscriptAFI = monoscriptDep.table.getAssetInfo((ulong)scriptPathId);
                        if (monoscriptAFI.curFileType == 115)
                        {
                            monoscriptDep.file.reader.Position = monoscriptAFI.absoluteFilePos;
                            monoscriptDep.file.reader.Align();

                            string monoscriptName = monoscriptDep.file.reader.ReadCountStringInt32();

                            if (monoscriptName == type.Name)
                            {
                                afr.Position = prevPos;
                                return componentAFI;
                            }
                        }
                        afr.Position = curPos;
                    }
                }
            }

            afr.Position = prevPos;

            return null;
        }

        public static Transform GetWorldTransform(AssetsFileInstance assetsFileInstance, ulong gameObjectPathId)
        {
            AssetsFileTable assetsTable = assetsFileInstance.table;
            AssetsFile assetsFile = assetsFileInstance.file;
            AssetsFileReader afr = assetsFile.reader;

            ulong prevPos = assetsFile.reader.Position;

            AssetFileInfoEx transformAsset = FindTransform(assetsFileInstance, gameObjectPathId);
            afr.Position = transformAsset.absoluteFilePos + 12;

            Quaternion rotation = new Quaternion(
                afr.ReadSingle(), // Quaternion X
                afr.ReadSingle(), // Quaternion Y
                afr.ReadSingle(), // Quaternion Z
                afr.ReadSingle()); // Quaternion W

            Vector3 position = new Vector3(
                afr.ReadSingle(), // Position X
                afr.ReadSingle(), // Position Y
                afr.ReadSingle()); // Position Z

            Vector3 scale = new Vector3(
                afr.ReadSingle(), // Scale X
                afr.ReadSingle(), // Scale Y
                afr.ReadSingle()); // Scale Z

            Transform transform = new Transform(position, scale, rotation); // establish our first transform
            int childrenCount = afr.ReadInt32();
            for (int i = 0; i < childrenCount; i++)
            {
                afr.Position += 12; //skip Children file and path ids
            }
            afr.ReadInt32();
            ulong transformPathId = (ulong)afr.ReadInt64();

            AssetFileInfoEx transformFatherAsset = assetsTable.getAssetInfo(transformPathId);
            while (transformPathId > 0)
            {
                ulong loopPos = assetsFile.reader.Position;
                afr.Position = transformFatherAsset.absoluteFilePos + 12;

                rotation = new Quaternion(
                    afr.ReadSingle(), // Quaternion X
                    afr.ReadSingle(), // Quaternion Y
                    afr.ReadSingle(), // Quaternion Z
                    afr.ReadSingle()); // Quaternion W

                position = new Vector3(
                    afr.ReadSingle(), // Position X
                    afr.ReadSingle(), // Position Y
                    afr.ReadSingle()); // Position Z

                scale = new Vector3(
                    afr.ReadSingle(), // Scale X
                    afr.ReadSingle(), // Scale Y
                    afr.ReadSingle()); // Scale Z

                transform.Position += position;

                childrenCount = afr.ReadInt32();
                for (int i = 0; i < childrenCount; i++)
                {
                    afr.Position += 12; //skip Children file and path ids
                }

                afr.ReadInt32();
                transformPathId = (ulong)afr.ReadInt64();

                transformFatherAsset = assetsTable.getAssetInfo(transformPathId);
            }
            return transform; // This should likely never happen
        }
    }
}
