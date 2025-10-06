#!/usr/bin/env python3
import hashlib
import json
import sys
import re
import os
import subprocess
from datetime import datetime
from urllib.request import urlopen
from urllib.error import HTTPError

REPO_URL = "https://github.com/Sokwva/AcJellyfun"

def generate_manifest():
    return    [{
        "guid": "e471526c-fc10-4c54-a769-3b8e85abf708",
        "name": "AcJellyfun",
        "description": "",
        "overview": "",
        "owner": "Sokwva",
        "category": "Metadata",
        "imageUrl": "",
        "versions": []
    }]

def generate_version(filepath, version, changelog):
    return {
        'version': f"{version}.0",
        'changelog': changelog,
        'targetAbi': '10.10.7.0',
        'sourceUrl': f'{REPO_URL}/releases/download/v{version}/AcJellyfun_{version}.0.zip',
        'checksum': md5sum(filepath),
        'timestamp': datetime.now().strftime('%Y-%m-%dT%H:%M:%S')
    }

def md5sum(filename):
    with open(filename, 'rb') as f:
        return hashlib.md5(f.read()).hexdigest()


def main():
    filename = sys.argv[1]
    tag = sys.argv[2]
    version = tag.lstrip('v')
    filepath = os.path.join(os.getcwd(), filename)
    result = subprocess.run(['git', 'tag','-l','--format=%(contents)', tag, '-l'], stdout=subprocess.PIPE)
    changelog = result.stdout.decode('utf-8').strip()

    try:
        with urlopen(f'{REPO_URL}/releases/download/manifest/manifest.json') as f:
            manifest = json.load(f)
    except HTTPError as err:
        if err.code == 404:
            manifest = generate_manifest()
        else:
            raise

    manifest[0]['versions'] = list(filter(lambda x: x['version'] != f"{version}.0", manifest[0]['versions']))
    manifest[0]['versions'].insert(0, generate_version(filepath, version, changelog))

    with open('manifest.json', 'w') as f:
        json.dump(manifest, f, indent=2)

    cn_domain = 'https://ghfast.top/'
    if 'CN_DOMAIN' in os.environ and os.environ["CN_DOMAIN"]:
        cn_domain = os.environ["CN_DOMAIN"]
    cn_domain = cn_domain.rstrip('/')
    with open('manifest_cn.json', 'w') as f:
        manifest_cn = json.dumps(manifest, indent=2)
        manifest_cn = re.sub('https://github.com', f'{cn_domain}/https://github.com', manifest_cn)
        f.write(manifest_cn)


if __name__ == '__main__':
    main()