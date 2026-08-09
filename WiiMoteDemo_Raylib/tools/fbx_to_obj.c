// Converts legacy ASCII or binary FBX meshes to a portable OBJ using ufbx.
// Compile this file together with ufbx.c: https://github.com/ufbx/ufbx
#include "ufbx.h"
#include <math.h>
#include <stdio.h>
#include <stdlib.h>

static ufbx_vec3 normalize(ufbx_vec3 v)
{
    double length = sqrt(v.x*v.x + v.y*v.y + v.z*v.z);
    if (length > 0.0) { v.x /= length; v.y /= length; v.z /= length; }
    return v;
}

int main(int argc, char **argv)
{
    if (argc != 3) { fprintf(stderr, "usage: fbx_to_obj input.fbx output.obj\n"); return 2; }

    ufbx_load_opts opts = { 0 };
    opts.generate_missing_normals = true;
    ufbx_error error;
    ufbx_scene *scene = ufbx_load_file(argv[1], &opts, &error);
    if (!scene) { fprintf(stderr, "FBX load failed: %s\n", error.description.data); return 1; }

    FILE *out = fopen(argv[2], "wb");
    if (!out) { ufbx_free_scene(scene); return 1; }
    fprintf(out, "# Converted from legacy FBX by ufbx\nmtllib wiimote.mtl\nusemtl WiiRemote\n");

    size_t vertex = 1;
    for (size_t ni = 0; ni < scene->nodes.count; ni++) {
        ufbx_node *node = scene->nodes.data[ni];
        ufbx_mesh *mesh = node->mesh;
        if (!mesh) continue;
        ufbx_matrix normal_matrix = ufbx_matrix_for_normals(&node->geometry_to_world);

        for (size_t fi = 0; fi < mesh->faces.count; fi++) {
            ufbx_face face = mesh->faces.data[fi];
            if (face.num_indices < 3) continue;
            uint32_t *triangles = (uint32_t*)malloc(sizeof(uint32_t) * (face.num_indices - 2) * 3);
            uint32_t triangle_count = ufbx_triangulate_face(triangles, (face.num_indices - 2) * 3, mesh, face);
            uint32_t count = triangle_count * 3;
            for (uint32_t ti = 0; ti < count; ti++) {
                uint32_t ix = triangles[ti];
                ufbx_vec3 p = ufbx_transform_position(&node->geometry_to_world, ufbx_get_vertex_vec3(&mesh->vertex_position, ix));
                ufbx_vec3 n = normalize(ufbx_transform_direction(&normal_matrix, ufbx_get_vertex_vec3(&mesh->vertex_normal, ix)));
                ufbx_vec2 uv = mesh->vertex_uv.exists ? ufbx_get_vertex_vec2(&mesh->vertex_uv, ix) : (ufbx_vec2){ 0, 0 };
                fprintf(out, "v %.9g %.9g %.9g\nvt %.9g %.9g\nvn %.9g %.9g %.9g\n", p.x,p.y,p.z, uv.x,1.0-uv.y, n.x,n.y,n.z);
            }
            for (uint32_t ti = 0; ti < count; ti += 3) {
                fprintf(out, "f %zu/%zu/%zu %zu/%zu/%zu %zu/%zu/%zu\n",
                    vertex,vertex,vertex, vertex+1,vertex+1,vertex+1, vertex+2,vertex+2,vertex+2);
                vertex += 3;
            }
            free(triangles);
        }
    }

    fclose(out);
    ufbx_free_scene(scene);
    return 0;
}
