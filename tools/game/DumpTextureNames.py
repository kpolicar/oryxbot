import os
import UnityPy
import sys

input_dir = sys.argv[1]
output_file = sys.argv[2]

names = []
for root, _, files in os.walk(input_dir):
    for file in files:
        if file.endswith('.assets') or file.endswith('.bundle'):
            file_path = os.path.join(root, file)
            try:
                env = UnityPy.load(file_path)
                for obj in env.objects:
                    if obj.type.name == "Sprite":
                        data = obj.read()
                        name = getattr(data, 'name', getattr(data, 'm_Name', None))
                        if name:
                            names.append(name.lower())
            except Exception:
                pass

with open(output_file, 'w') as f:
    for name in sorted(set(names)):
        f.write(name + '\n')
print(f"Dumped {len(names)} unique texture names to {output_file}")
