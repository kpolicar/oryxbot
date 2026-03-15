import UnityPy
env = UnityPy.load('/home/oryxbot/game/full/Albion-Online_Data/resources.assets')
count = 0
for obj in env.objects:
    if obj.type.name == 'Texture2D':
        try:
            data = obj.read()
            name = getattr(data, 'name', getattr(data, 'm_Name', 'unknown'))
            print(name)
            count += 1
            if count > 20: break
        except Exception as e:
            print(f"Error reading: {e}")
