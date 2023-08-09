using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AOBakery : EditorWindow
{
    #region member variables

    public GameObject target;
    public Material replacedMat;
    public int samples = 512;
    public int subdivisions = 1;
    public float maxRange = 1.5f;
    public float minRange = 0.0000000001f;
    public float intensity = 2.0f;
    public float power = 1.0f;
    //public bool testBake = false;
    public bool clearVertexColors = false;
    public bool smoothResults = false;

    private static Texture button_tex;
    private static GUIContent button_tex_con;
    private static string[] messages = { "Baking muffins...", "Baking marshmallows", "Putting cookies in the oven" };
    private string bakingMessage;
    private string estimation = "Estimate";

    #endregion

    [MenuItem("Tools/Open Bakery")]
    static void Init()
    {
        if (!AssetDatabase.IsValidFolder("Assets/AOBakery/ClonedMeshes"))
            AssetDatabase.CreateFolder("Assets/AOBakery", "ClonedMeshes");

        if (EditorWindow.GetWindow(typeof(AOBakery)))
        {
            AOBakery w = (AOBakery)EditorWindow.GetWindow(typeof(AOBakery));
            w.Close();
        }

        button_tex = (Texture)AssetDatabase.LoadAssetAtPath("Assets/AOBakery/cake.png", typeof(Texture));
        button_tex_con = new GUIContent(button_tex);
        AOBakery window = (AOBakery)EditorWindow.GetWindow(typeof(AOBakery));
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Bake Settings", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical();
        GUILayout.Label("GameObject to bake");
        target = (GameObject)EditorGUILayout.ObjectField(target, typeof(GameObject), true);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        samples = EditorGUILayout.IntSlider("Samples", samples, 1, 2048);
        subdivisions = EditorGUILayout.IntSlider("Mesh subdivisions", subdivisions, 1, 10);
        minRange = EditorGUILayout.Slider("Min Distance", minRange, 0.000001f, 10f);
        maxRange = EditorGUILayout.Slider("Max Distance", maxRange, 0.1f, 100f);
        intensity = EditorGUILayout.Slider("Intensity", intensity, 0.1f, 10f);
        power = EditorGUILayout.Slider("Power", power, 1f, 10f);

        GUILayout.Label("Optional Settings", EditorStyles.boldLabel);
        GUILayout.Label("Replacement Material");
        replacedMat = (Material)EditorGUILayout.ObjectField(replacedMat, typeof(Material), true);
        clearVertexColors = EditorGUILayout.Toggle("Clear Vertex Colors", clearVertexColors);
        smoothResults = EditorGUILayout.Toggle("Smooth Results", smoothResults);
        //if (subdivisions == 1)
        //{
        //    GUILayout.Label("Test Bake will reset AO if the scene changes");
        //    testBake = EditorGUILayout.Toggle("Test Bake", testBake);
        //}
        //else
        //{
        //    GUILayout.Label("Test Bakes cannot work with subdivisions");
        //    testBake = false;
        //}
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        if (target != null)
        {
            if (GUILayout.Button(estimation, GUILayout.Height(20), GUILayout.ExpandWidth(true)))
                Estimate();

            if (button_tex_con != null)
            {
                if (GUILayout.Button(button_tex_con, GUILayout.Height(100), GUILayout.ExpandWidth(true)))
                    Bake();
            }
            else
            {
                //refresh window
                Init();
            }
        }
        else
        {
            GUILayout.Label("You need to set a target for the bake!", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
        }
        EditorGUILayout.EndVertical();
    }

    void Estimate()
    {
        int verts = 0;
        MeshFilter[] mfs = target.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter mf in mfs)
        {
            Mesh clone = new Mesh();
            string meshName = mf.name.Replace("clone_", "");
            Mesh backup = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/AOBakery/ClonedMeshes/" + GetVanillaMeshName(meshName) + "/backup_" + meshName + ".asset", typeof(Mesh));
            if (backup != null)
            {
                clone.vertices = backup.vertices;
                clone.normals = backup.normals;
                clone.tangents = backup.tangents;
                clone.triangles = backup.triangles;
                clone.uv = backup.uv;
                clone.colors = backup.colors;
            }
            else
            {
                //create subdivided mesh
                clone.vertices = mf.sharedMesh.vertices;
                clone.normals = mf.sharedMesh.normals;
                clone.tangents = mf.sharedMesh.tangents;
                clone.triangles = mf.sharedMesh.triangles;
                clone.uv = mf.sharedMesh.uv;
                clone.colors = mf.sharedMesh.colors;
            }
            MeshHelper.Subdivide(clone, subdivisions);
            verts += clone.vertices.Length;
        }

        float timeInMinutes = (((float)verts * samples) / 1000f) / 60f;

        estimation = "Vertices: " + verts + ", Minutes: " + (timeInMinutes / 100f).ToString("0.##");
    }

    void Bake()
    {
        if (target == null)
        {
            Debug.LogWarning("You need to set a target for the bake!");
            return;
        }

        bakingMessage = messages[UnityEngine.Random.Range(0, messages.Length)];

        EditorUtility.ClearProgressBar();

        RaycastHit hit = new RaycastHit();

        MeshFilter[] mfs = target.GetComponentsInChildren<MeshFilter>();
        MeshRenderer[] mrs = target.GetComponentsInChildren<MeshRenderer>();

        int sample = 0;
        int numVerts = 0;
        foreach (MeshFilter mf in mfs)
            numVerts += mf.sharedMesh.vertices.Length;
        int numSamples = numVerts * samples;

        if (replacedMat != null)
        {
            foreach (MeshRenderer mr in mrs)
            {
                Material[] intMaterials = new Material[mr.sharedMaterials.Length];
                for (int i = 0; i < intMaterials.Length; i++)
                {
                    intMaterials[i] = replacedMat;
                }
                mr.sharedMaterials = intMaterials;

            }
        }

        // Iterate over all meshes
        foreach (MeshFilter mf in mfs)
        {
            Mesh mesh = mf.sharedMesh;
            Mesh clone = new Mesh();
            Mesh backup = new Mesh();
            string meshName = mesh.name.Replace("clone_", "");

            clone = mesh;

            backup = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/AOBakery/ClonedMeshes/" + meshName + "/backup_" + meshName + ".asset", typeof(Mesh));
            if (backup != null)
            {
                clone = new Mesh();
                clone.vertices = backup.vertices;
                clone.normals = backup.normals;
                clone.tangents = backup.tangents;
                clone.triangles = backup.triangles;
                clone.uv = backup.uv;
                clone.colors = backup.colors;
            }
            else
            {
                clone.vertices = mesh.vertices;
                clone.normals = mesh.normals;
                clone.tangents = mesh.tangents;
                clone.triangles = mesh.triangles;
                clone.uv = mesh.uv;
                clone.colors = mesh.colors;

                backup = new Mesh();
                //save backup
                backup.vertices = clone.vertices;
                backup.normals = clone.normals;
                backup.tangents = clone.tangents;
                backup.triangles = clone.triangles;
                backup.uv = clone.uv;
                backup.colors= clone.colors;
            }


            //subdivide!
            if (subdivisions > 1)
                MeshHelper.Subdivide(clone, subdivisions);

            //apply mesh to mesh collider too!
            if (mf.GetComponent<MeshCollider>())
                mf.GetComponent<MeshCollider>().sharedMesh = clone;

            // Store vertices
            Vector3[] verts = clone.vertices;

            // Store colors
            Color[] colors = mesh.colors;
            if (colors.Length == 0 || colors.Length != verts.Length)
            {
                colors = new Color[verts.Length];
            }

            //init colors
            for (int ic = 0; ic < colors.Length; ic++)
            {
                colors[ic].a = 1;
                if (clearVertexColors)
                {
                    colors[ic].r = 1;
                    colors[ic].g = 1;
                    colors[ic].b = 1;
                }
            }

            // Store normals
            Vector3[] normals = new Vector3[clone.normals.Length];
            if (normals.Length == 0 || normals.Length != verts.Length)
                clone.RecalculateNormals();

            if (smoothResults)
            {
                clone.RecalculateBounds();
                clone.RecalculateNormals();
                normals = clone.normals;
            }
            else
            {
                normals = clone.normals;
            }

            // Loop over the verts and perform AO
            int i, j, l = 0;
            l = verts.Length;

            for (i = 0; i < l; i++)
            {
                Vector3 nrm = normals[i];
                Vector3 vertWrld = target.transform.TransformPoint(verts[i]);
                Vector3 n = target.transform.TransformPoint(verts[i] + nrm);
                Vector3 world = (n - vertWrld);
                world.Normalize();

                float occlusion = 0;

                for (j = 0; j < samples; j++)
                {
                    float rot = 180.0f;
                    float rot2 = rot / 2.0f;
                    float rotx = ((rot * UnityEngine.Random.value) - rot2);
                    float roty = ((rot * UnityEngine.Random.value) - rot2);
                    float rotz = ((rot * UnityEngine.Random.value) - rot2);
                    Vector3 dir = Quaternion.Euler(rotx, roty, rotz) * Vector3.up;
                    Quaternion dirq = Quaternion.FromToRotation(Vector3.up, world);

                    Vector3 ray = dirq * dir;
                    Vector3 offset = Vector3.Reflect(ray, world);

                    ray = ray * (maxRange / ray.magnitude);
                    if (Physics.Linecast(vertWrld - (offset * .1f), vertWrld + ray, out hit))
                    {
                        if (hit.distance > minRange)
                        {
                            occlusion += Mathf.Clamp01(1 - (hit.distance / maxRange));
                        }
                    }

                    // Update progress bar
                    if (++sample % 500 == 0)
                    {
                        EditorUtility.DisplayProgressBar(
                            "AO Bakery",
                            bakingMessage,
                            (float)sample / (float)numSamples
                        );
                    }
                }

                occlusion = Mathf.Pow(Mathf.Clamp01(1 - ((occlusion * intensity) / samples)), power);
                colors[i].a = colors[i].a * occlusion;
            }

            clone.colors = colors;

            //save
            Mesh meshToSave = UnityEngine.Object.Instantiate(clone) as Mesh;

            var guid = "";
            var newGuid = "~" + Guid.NewGuid().ToString();
            Debug.Log(name.Split('~').Length);
            string folderName = meshName.Split('~').Length == 1 ? meshName + newGuid : meshName;

            if (!AssetDatabase.IsValidFolder("Assets/AOBakery/ClonedMeshes/" + folderName))
            {
                AssetDatabase.CreateFolder("Assets/AOBakery/ClonedMeshes", folderName);
            }

            if (AssetDatabase.LoadAssetAtPath("Assets/AOBakery/ClonedMeshes/" + folderName + "/clone_" + meshName + guid + ".asset", typeof(Mesh)) == null)
            {
                guid = newGuid;
            }
            AssetDatabase.CreateAsset(meshToSave, "Assets/AOBakery/ClonedMeshes/" + folderName + "/clone_" + meshName + guid + ".asset");
            AssetDatabase.SaveAssets();

            mf.sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/AOBakery/ClonedMeshes/" + folderName + "/clone_" + meshName + guid + ".asset", typeof(Mesh));
            //}

            //save backup anyways (the first time)
            if (!AssetDatabase.IsValidFolder("Assets/AOBakery/ClonedMeshes/" + folderName))
                AssetDatabase.CreateFolder("Assets/AOBakery/ClonedMeshes", folderName);
            if (AssetDatabase.LoadAssetAtPath("Assets/AOBakery/ClonedMeshes/" + folderName + "/backup_" + meshName + guid + ".asset", typeof(Mesh)) == null)
            {
                guid = newGuid;

                AssetDatabase.CreateAsset(backup, "Assets/AOBakery/ClonedMeshes/" + folderName + "/backup_" + meshName + guid + ".asset");
                AssetDatabase.SaveAssets();
            }

        }

        EditorUtility.ClearProgressBar();
    }

    protected string GetVanillaMeshName(string name)
    {
        var pieces = name.Split('~');
        var actualName = pieces[0].Replace("clone_", "");
        return actualName;
    }

    //from: https://answers.unity.com/questions/58692/UnityEngine.Randomonunitsphere-but-within-a-defined-range.html?_ga=2.104097216.1786208018.1589286556-861077915.1572618047
    public static Vector3 GetPointOnUnitSphereCap(Quaternion targetDirection, float angle)
    {
        var angleInRad = UnityEngine.Random.Range(0.0f, angle) * Mathf.Deg2Rad;
        var PointOnCircle = (UnityEngine.Random.insideUnitCircle.normalized) * Mathf.Sin(angleInRad);
        var V = new Vector3(PointOnCircle.x, PointOnCircle.y, Mathf.Cos(angleInRad));
        return targetDirection * V;
    }
    public static Vector3 GetPointOnUnitSphereCap(Vector3 targetDirection, float angle)
    {
        return GetPointOnUnitSphereCap(Quaternion.LookRotation(targetDirection), angle);
    }
}
