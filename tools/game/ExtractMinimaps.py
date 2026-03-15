import os
import UnityPy
import argparse

def extract_minimaps(input_dir, output_dir):
    print(f"Starting extraction from {input_dir}")
    print(f"Outputting to {output_dir}")
    
    if not os.path.exists(output_dir):
        os.makedirs(output_dir)

    # Find all .assets and .bundle files
    files_to_process = []
    for root, _, files in os.walk(input_dir):
        for file in files:
            if file.endswith('.assets') or file.endswith('.bundle'):
                files_to_process.append(os.path.join(root, file))

    print(f"Found {len(files_to_process)} asset/bundle files.")

    extracted_count = 0
    for file_path in files_to_process:
        try:
            env = UnityPy.load(file_path)
            for obj in env.objects:
                if obj.type.name == "Texture2D":
                    data = obj.read()
                    name = getattr(data, 'name', getattr(data, 'm_Name', 'unknown')).lower()
                    
                    target_names = [
                        'minimapatlas',
                        'worldmap',
                        'worldmap_upscaled',
                        'worldmap_props',
                        'map_albion',
                        'map_albion_forest',
                        'map_albion_highlands',
                        'map_albion_mountain',
                        'map_albion_steppe',
                        'map_albion_swamp',
                        'worldmap_outlands_teleportation_portal'
                    ]
                    
                    if name in target_names:
                        try:
                            img = data.image
                            
                            out_path = os.path.join(output_dir, f"{name}.png")
                            img.save(out_path)
                            extracted_count += 1
                            print(f"Extracted: {name}.png")
                        except Exception as img_err:
                            print(f"Failed to decode image data for {name} in {file_path}: {img_err}")
                            pass
        except Exception as e:
            # We skip files that can't be read by UnityPy
            pass

    print(f"Extraction complete. Extracted {extracted_count} minimap textures.")

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description="Extract Albion Online minimap textures.")
    parser.add_argument('--input', type=str, required=True, help="Path to Albion-Online_Data directory")
    parser.add_argument('--output', type=str, required=True, help="Directory to save extracted .png files")
    
    args = parser.parse_args()
    
    extract_minimaps(args.input, args.output)
