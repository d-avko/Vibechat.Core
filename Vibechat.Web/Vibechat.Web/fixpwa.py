import json

with open('locales.json', 'r+') as locales:
    localesObject = json.load(locales)
    for attribute, value in localesObject.items():
        try:
            with open(value, 'r+') as specificLocale:
                manifest = json.load(specificLocale)
                manifest['scope'] = '/'+ attribute + '/'
                manifest['start_url'] = '/' + attribute + '/'
                specificLocale.seek(0)
                # pretty-print the manifest
                json.dump(manifest, specificLocale, indent=4)
        except Exception as e:
            print(e)
            print("Error while opening file for locale " + attribute)
