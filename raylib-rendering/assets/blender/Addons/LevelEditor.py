bl_info = {
    "name": "Dropper",
    "description": "Saves info about objects in the scene.",
    "author": "Joseph Hocking www.newarteest.com",
    "blender": (2, 80, 0),
    "location": "File > Export > Dropper",
    "category": "Import-Export",
}

'''
install in Edit > Preferences > Add-ons
once installed: File > Export > Dropper
GPL license https://www.blender.org/support/faq/

Saves info about objects in the scene:
name, position, rotation, scale, custom properties
Select either XML or JSON for the data format.
'''


import bpy
import json

# favor the accelerated C implementation, fallback to Python implementation
try:
    import xml.etree.cElementTree as et
except ImportError:
    import xml.etree.ElementTree as et
# http://eli.thegreenplace.net/2012/03/15/processing-xml-in-python-with-elementtree


def write_data(context, filepath, format):
    print("writing scene data...")
    
    data = {
        'OPT_JSON': build_json(),
        # 'OPT_XML': build_xml(),
    }[format]
    
    f = open(filepath, 'w', encoding='utf-8')
    f.write(data)
    f.close()

    return {'FINISHED'}
#----- end func -----

def pretty_float_json_dumps(json_obj):
    dumps_str = ""

    if isinstance(json_obj, dict): 
        dumps_str += "{"
        for k,v in json_obj.items():
            dumps_str += json.dumps(k)+":"
            if isinstance(v, float): 
                float_tmp_str = ("%.16f" % v).rstrip("0")
                dumps_str += (float_tmp_str+'0' if float_tmp_str.endswith('.') else float_tmp_str) + ','
            elif isinstance(v, list) or isinstance(v, dict): 
                dumps_str += pretty_float_json_dumps(v)+','
            else:
                dumps_str += pretty_float_json_dumps(v)+','
        if dumps_str.endswith(','):
            dumps_str = dumps_str[:-1]
        dumps_str += "}"
    elif isinstance(json_obj, list): 
        dumps_str += "["
        for v in json_obj:
            if isinstance(v, float): 
                float_tmp_str = ("%.16f" % v).rstrip("0")
                dumps_str += (float_tmp_str+'0' if float_tmp_str.endswith('.') else float_tmp_str) + ','
            elif isinstance(v, list) or isinstance(v, dict): 
                dumps_str += pretty_float_json_dumps(v)+','
            else:
                dumps_str += pretty_float_json_dumps(v)+','
        if dumps_str.endswith(','):
            dumps_str = dumps_str[:-1]
        dumps_str += "]"
    else:
        dumps_str += json.dumps(json_obj)
    return dumps_str


def build_json():
    dict = {}
    dict["entities"] = []

    for obj in bpy.data.objects:

        print(obj.name, "s parent is", obj.users_collection)
        if obj.users_collection[0].name == "Collection":
            print("adding object", obj.name)
            recursiveAddObject(obj, dict["entities"])


    return pretty_float_json_dumps(dict)


def recursiveAddObject(obj, listToAdd):
    entity = {"name":obj.name}
    entity["position"] = {"x":obj.location.x + obj.delta_location.x, "y":obj.location.y + obj.delta_location.y, "z":obj.location.z + obj.delta_location.z}
    entity["rotation"] = {"x":obj.rotation_euler.x, "y":obj.rotation_euler.y, "z":obj.rotation_euler.z}
    entity["scale"] = {"x":obj.scale.x, "y":obj.scale.y, "z":obj.scale.z}
    entity["children"] = []

    collectionInstance = obj.instance_collection

    if collectionInstance:
        print(collectionInstance.objects)
        for child in collectionInstance.objects:
            recursiveAddObject(child, entity["children"])

    print(obj.name, obj.type)

    if obj.type == "CURVE":
        # get curve data
        curve = obj.data

        # get every connected point in the curve
        lines = []
        for spline in curve.splines:
            points = []
            for point in spline.points:
                points.append({"x":point.co.x, "y":point.co.y, "z":point.co.z})
            lines.append(points)
        entity["lines"] = lines

    # custom properties
    for k in obj.keys():
        prop = obj[k]
        if isinstance(prop, int) or isinstance(prop, float) or isinstance(prop, str):
            entity[k] = obj[k]
    listToAdd.append(entity)


def build_xml():
    root = et.Element("entities")
    
    for obj in bpy.data.objects:
        e = et.Element("entity")
        et.SubElement(e, "name").text = obj.name
        et.SubElement(e, "position").attrib = {"x":str(obj.location.x), "y":str(obj.location.y), "z":str(obj.location.z)}
        et.SubElement(e, "rotation").attrib = {"x":str(obj.rotation_euler.x), "y":str(obj.rotation_euler.y), "z":str(obj.rotation_euler.z)}
        et.SubElement(e, "scale").attrib = {"x":str(obj.scale.x), "y":str(obj.scale.y), "z":str(obj.scale.z)}
        # custom properties
        if len(obj.keys()) > 1:
            for k in obj.keys():
                if k not in "_RNA_UI":
                    prop = obj[k]
                    if isinstance(prop, int) or isinstance(prop, float) or isinstance(prop, str):
                        et.SubElement(e, k).text = str(obj[k])
        root.append(e)
    
    indent(root)
    return et.tostring(root).decode("utf-8")

#http://effbot.org/zone/element-lib.htm#prettyprint
def indent(elem, level=0):
    i = "\n" + level*"    "
    if len(elem):
        if not elem.text or not elem.text.strip():
            elem.text = i + "    "
        if not elem.tail or not elem.tail.strip():
            elem.tail = i
        for elem in elem:
            indent(elem, level+1)
        if not elem.tail or not elem.tail.strip():
            elem.tail = i
    else:
        if level and (not elem.tail or not elem.tail.strip()):
            elem.tail = i
#----- end func -----



#----- begin gui -----

# ExportHelper is a helper class, defines filename and
# invoke() function which calls the file selector.
from bpy_extras.io_utils import ExportHelper
from bpy.props import StringProperty, BoolProperty, EnumProperty
from bpy.types import Operator


class Dropper(Operator, ExportHelper):
    """Dropper - Saves info about objects in the scene."""
    bl_idname = "dropper.scene_text"
    bl_label = "Export Scene Data"

    # ExportHelper mixin class uses this
    filename_ext = ".json"

    filter_glob: StringProperty(
        default="*.json",
        options={'HIDDEN'},
        maxlen=255,  # Max internal buffer length, longer would be clamped.
    )

    # options menu next to the file selector
    data_format: EnumProperty(
        name="Data Format",
        description="Choose the data format",
        items=(('OPT_JSON', "JSON", "JavaScript Object Notation"),),
        default='OPT_JSON',
    )

    def execute(self, context):
        return write_data(context, self.filepath, self.data_format)


def menu_func_export(self, context):
    self.layout.operator(Dropper.bl_idname, text="Dropper")


def register():
    bpy.utils.register_class(Dropper)
    bpy.types.TOPBAR_MT_file_export.append(menu_func_export)


def unregister():
    bpy.utils.unregister_class(Dropper)
    bpy.types.TOPBAR_MT_file_export.remove(menu_func_export)


if __name__ == "__main__":
    register()

    # test call
    #bpy.ops.dropper.scene_text('INVOKE_DEFAULT')
