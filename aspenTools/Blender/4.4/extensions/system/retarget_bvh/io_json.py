# SPDX-FileCopyrightText: 2019-2025, Thomas Larsson
#
# SPDX-License-Identifier: GPL-2.0-or-later

import json
import gzip
import os
from .utils import MocapError

def loadJson(filepath):
    try:
        with gzip.open(filepath, 'rb') as fp:
            bytes = fp.read()
    except IOError:
        bytes = None

    def getDecoded(utf):
        if bytes:
            string = bytes.decode(utf)
            return json.loads(string)
        else:
            with open(filepath, "r", encoding=utf) as fp:
                return json.load(fp)

    try:
        try:
            struct = getDecoded("utf-8-sig")
        except UnicodeDecodeError:
            struct = getDecoded("utf-16")
        msg = ""
    except json.decoder.JSONDecodeError as err:
        msg = ('JSON error while reading file\n"%s"\n%s' % (filepath, err))
    except UnicodeDecodeError:
        msg = ('Unicode error while reading file\n"%s"\n%s' % (filepath, err))
    except:
        msg = ("Could not load %s" % filepath)
    if msg:
        raise MocapError(msg)
    return struct


def saveJson(struct, filepath, binary=False):
    folder = os.path.dirname(filepath)
    if not os.path.exists(folder):
        os.makedirs(folder)
    if binary:
        bytes = json.dumps(struct)
        with gzip.open(realpath, 'wb') as fp:
            fp.write(bytes)
    else:
        string = encodeJsonData(struct, "")
        with open(filepath, "w", encoding="utf-8-sig") as fp:
            fp.write(string)
            fp.write("\n")


def encodeJsonData(data, pad=""):
    if data == None:
        return "none"
    elif isinstance(data, bool):
        if data == True:
            return "true"
        else:
            return "false"
    elif isinstance(data, float):
        if abs(data) < 1e-6:
            return "0"
        else:
            return "%.5g" % data
    elif isinstance(data, int):
        return str(data)
    elif isinstance(data, str):
        return "\"%s\"" % data
    elif isinstance(data, (list, tuple)):
        if data == []:
            return "[]"
        elif leafList(data):
            string = "["
            for elt in data:
                string += encodeJsonData(elt) + ", "
            return string[:-2] + "]"
        else:
            string = "["
            for elt in data:
                string += "\n    " + pad + encodeJsonData(elt, pad+"    ") + ","
            return string[:-1] + "\n%s]" % pad
    elif isinstance(data, dict):
        if data == {}:
            return "{}"
        string = "{"
        for key,value in data.items():
            string += "\n    %s\"%s\" : " % (pad, key) + encodeJsonData(value, pad+"    ") + ","
        return string[:-1] + "\n%s}" % pad


def leafList(data):
    for elt in data:
        if isinstance(elt, (list,tuple,dict)):
            return False
    return True
