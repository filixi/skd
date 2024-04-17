using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpriteManager
{
    public struct AtlasMapping
    {
        public int atlas_id;
        public int entity_id;
        public string entity_name;
        public Vector4 render_data;
    };

    public class EntityAtlas
    {
        public int atlas_id = 0;
        public Texture2D texture;
        public AtlasMapping[] atlas_mapping;
    }

    Shader flipbook_shader;

    public class EntityMaterialEntry
    {
        public Dictionary<int, EntityAtlas> atlas = new Dictionary<int, EntityAtlas>();
        public Texture2DArray texture_array;
        public Material array_material;
        public Dictionary<string, AtlasMapping> id_mapping = new Dictionary<string, AtlasMapping>();
    }

    public EntityMaterialEntry entity_material_entry = new EntityMaterialEntry();

    private void SetupMaterial(EntityMaterialEntry entry, int dimention)
    {
        foreach (var kv in entry.atlas)
        {
            foreach (var atlas in kv.Value.atlas_mapping)
                entry.id_mapping.Add(atlas.entity_name, atlas);
        }

        var indexes = entry.atlas.Keys.ToList();
        indexes.Sort();
        for (int i = 0; i < indexes.Count; ++i)
        {
            if (indexes[i] != i)
                throw new ArgumentException("Invalid atlas id, must be continuous from 0");
        }

        Texture2DArray ta = null;
        foreach (var id in indexes)
        {
            var e = entry.atlas[id];
            if (ta == null)
            {
                ta = new Texture2DArray(
                        e.texture.width,
                        e.texture.height,
                        indexes.Count,
                        e.texture.format,
                        e.texture.mipmapCount > 1
                    );
            }
            Graphics.CopyTexture(e.texture, 0, 0, ta, id, 0);
        }

        entry.texture_array = ta;
        entry.array_material = new Material(flipbook_shader);
        entry.array_material.enableInstancing = true;
        entry.array_material.SetTexture("_MainTex", ta);
        entry.array_material.SetVector("_Clip", new Vector4(dimention, dimention, 0, 0));
    }

    private void InitializeEntity()
    {
        {
            var ma = new EntityAtlas();
            ma.atlas_id = 0;
            ma.texture = Resources.Load<Texture2D>("Sprite/entities");
            ma.atlas_mapping = new AtlasMapping[]
                {
                    new AtlasMapping { atlas_id = ma.atlas_id, entity_id = 1, entity_name = "Dog", render_data = new Vector4(4, 3, 0, 8) },
                    new AtlasMapping { atlas_id = ma.atlas_id, entity_id = 2, entity_name = "CloseCombat", render_data = new Vector4(4, 5, 0, 16) },
                    new AtlasMapping { atlas_id = ma.atlas_id, entity_id = 3, entity_name = "Range", render_data = new Vector4(4, 5, 0, 0) },
                    new AtlasMapping { atlas_id = ma.atlas_id, entity_id = 4, entity_name = "Safe", render_data = new Vector4(4, 4, 0, 9) },
                    new AtlasMapping { atlas_id = ma.atlas_id, entity_id = 5, entity_name = "Explosion", render_data = new Vector4(3, 3, 0, 20) },
                    new AtlasMapping { atlas_id = ma.atlas_id, entity_id = 6, entity_name = "Blade", render_data = new Vector4(3, 3, 0, 23) },
                    new AtlasMapping { atlas_id = ma.atlas_id, entity_id = 7, entity_name = "Laser", render_data = new Vector4(4, 3, 0, 26) },
                };
            entity_material_entry.atlas.Add(ma.atlas_id, ma);
        }

        SetupMaterial(entity_material_entry, 32);
    }

    public void Initialize()
    {
        flipbook_shader = Resources.Load<Shader>("Material/Flipbook");

        InitializeEntity();
    }
}
