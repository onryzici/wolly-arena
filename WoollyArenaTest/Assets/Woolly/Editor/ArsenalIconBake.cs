using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace WoollyArena.Editor
{
    public static class ArsenalIconBake
    {
        // Render the actual equipment, so shop art matches the silhouette seen in combat.
        public static void Bake()
        {
            const string folder="Assets/Woolly/Resources/WeaponIcons";Directory.CreateDirectory(folder);
            var shader=Resources.Load<Shader>("CombatModel");var metal=new Material(shader){color=new Color(.22f,.30f,.39f)};var wood=new Material(shader){color=new Color(.44f,.23f,.12f)};var brass=new Material(shader){color=new Color(.95f,.69f,.28f)};var glow=new Material(shader);
            var cameraObject=new GameObject("Weapon icon camera");var camera=cameraObject.AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.cullingMask=1<<30;camera.nearClipPlane=.1f;camera.farClipPlane=10;camera.allowHDR=false;camera.allowMSAA=true;
            var data=camera.GetUniversalAdditionalCameraData();data.renderPostProcessing=false;data.antialiasing=AntialiasingMode.None;
            var target=new RenderTexture(256,256,24,RenderTextureFormat.ARGB32){antiAliasing=4};var picture=new Texture2D(256,256,TextureFormat.RGBA32,false);var active=RenderTexture.active;
            try{
                foreach(var item in RunCatalog.All){
                    if(!item.IsWeapon||(int)item.Weapon.Value<6)continue;
                    var root=new GameObject("Icon model");root.transform.position=new Vector3(100,100,100);glow.color=WeaponArsenal.ColorFor(item.Weapon.Value);
                    try{
                        ArsenalModels.Build(root.transform,item.Weapon.Value,metal,wood,brass,glow);var bounds=new Bounds(root.transform.position,Vector3.zero);foreach(var renderer in root.GetComponentsInChildren<Renderer>()){renderer.gameObject.layer=30;bounds.Encapsulate(renderer.bounds);}
                        var direction=new Vector3(1,1.6f,-1.2f).normalized;camera.transform.position=bounds.center+direction*3;camera.transform.LookAt(bounds.center);camera.orthographicSize=bounds.size.magnitude*.57f;camera.targetTexture=target;
                        camera.Render();RenderTexture.active=target;picture.ReadPixels(new Rect(0,0,256,256),0,0);picture.Apply();string path=folder+"/"+item.Id+".png";File.WriteAllBytes(path,picture.EncodeToPNG());
                        AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
                    }finally{Object.DestroyImmediate(root);}
                }
            }finally{RenderTexture.active=active;camera.targetTexture=null;Object.DestroyImmediate(cameraObject);Object.DestroyImmediate(target);Object.DestroyImmediate(picture);Object.DestroyImmediate(metal);Object.DestroyImmediate(wood);Object.DestroyImmediate(brass);Object.DestroyImmediate(glow);}
        }
    }
}
