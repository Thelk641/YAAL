from worlds.LauncherComponents import Component, components, icon_paths, launch_subprocess, Type

def launch_client(*args):
    from CommonClient import gui_enabled
    if not gui_enabled:
        print(args)
        launch(args)
    launch_subprocess(launch, name="YAAL" args=args)

def launch(args):
    import os
    from Patch import create_rom_file

    # Accept either a list of args or a single string
    if isinstance(args, (list, tuple)):
        patch_file = args[0] if args else None
    else:
        patch_file = args

    if not patch_file:
        print("No patch file provided.")
        return

    patch_file = os.path.abspath(patch_file)

    try:
        meta_data, result_file = create_rom_file(patch_file)
        print(f"Patch created at {result_file} with metadata: {meta_data}")
    except Exception as e:
        print(f"Failed to patch file: {e}")


components.append(Component("YAAL Patcher", "YAAL Patcher", func=launch_client,
                            component_type=Type.CLIENT, supports_uri=True, game_name=None))