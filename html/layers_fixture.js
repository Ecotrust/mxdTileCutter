app.fixture = {
    "layers": [
        {
            "name": "treesummary", "url": ["treesummary/${z}/${x}/${y}.png"], // Changeme

            "opacity": 1.0, "lookups": {"field": null, "details": []}, "graphic": null, 
            "default_on": false, "data_source": "Ecotrust", "subLayers": [],
            "utfurl": null, "description": null, "arcgis_layers": null, "legend": null, "legend_title": null, 
            "attributes": {"attributes": [], "compress_attributes": false, "event": "click", "title": null}, 
            "fill_opacity": null, "learn_link": null, "type": "XYZ", "id": 1, "color": null, 
            "legend_subtitle": null
        },
        {
            "name": "fedland", "url": ["fedland/${z}/${x}/${y}.png"], "id": 2, // Changeme
            
            "opacity": 1.0, "lookups": {"field": null, "details": []}, "graphic": null, 
            "default_on": false, "data_source": "Ecotrust", "subLayers": [],
            "utfurl": null, "description": null, "arcgis_layers": null, "legend": null, "legend_title": null, 
            "attributes": {"attributes": [], "compress_attributes": false, "event": "click", "title": null}, 
            "fill_opacity": null, "learn_link": null, "type": "XYZ",  "color": null, 
            "legend_subtitle": null
        },
        {
            "name": "Counties", "url": ["Counties/${z}/${x}/${y}.png"], "id": 3, // Changeme
            
            "opacity": 1.0, "lookups": {"field": null, "details": []}, "graphic": null, 
            "default_on": false, "data_source": "Ecotrust", "subLayers": [],
            "utfurl": null, "description": null, "arcgis_layers": null, "legend": null, "legend_title": null, 
            "attributes": {"attributes": [], "compress_attributes": false, "event": "click", "title": null}, 
            "fill_opacity": null, "learn_link": null, "type": "XYZ",  "color": null, 
            "legend_subtitle": null
        },
        {
            "name": "Wilderness_Areas_5.18.10", "url": ["Wilderness_Areas_5.18.10/${z}/${x}/${y}.png"], "id": 4, // Changeme
            
            "opacity": 1.0, "lookups": {"field": null, "details": []}, "graphic": null, 
            "default_on": false, "data_source": "Ecotrust", "subLayers": [],
            "utfurl": null, "description": null, "arcgis_layers": null, "legend": null, "legend_title": null, 
            "attributes": {"attributes": [], "compress_attributes": false, "event": "click", "title": null}, 
            "fill_opacity": null, "learn_link": null, "type": "XYZ",  "color": null, 
            "legend_subtitle": null
        },
        {
            "name": "Countries", "url": ["esri_country/${z}/${x}/${y}.png"], "id": 5, // Changeme
            
            "opacity": 1.0, "lookups": {"field": null, "details": []}, "graphic": null, 
            "default_on": false, "data_source": "Ecotrust", "subLayers": [],
            "utfurl": null, "description": null, "arcgis_layers": null, "legend": null, "legend_title": null, 
            "attributes": {"attributes": [], "compress_attributes": false, "event": "click", "title": null}, 
            "fill_opacity": null, "learn_link": null, "type": "XYZ",  "color": null, 
            "legend_subtitle": null
        },
        {
            "name": "Watersheds", "url": ["pu_Final/${z}/${x}/${y}.png"], "id": 5, // Changeme
            
            "opacity": 1.0, "lookups": {"field": null, "details": []}, "graphic": null, 
            "default_on": false, "data_source": "Ecotrust", "subLayers": [],
            "utfurl": null, "description": null, "arcgis_layers": null, "legend": null, "legend_title": null, 
            "attributes": {"attributes": [], "compress_attributes": false, "event": "click", "title": null}, 
            "fill_opacity": null, "learn_link": null, "type": "XYZ",  "color": null, 
            "legend_subtitle": null
        },
    ],
    "themes": [{
        "layers": [1, 2, 3, 4, 5], // TODO, list all 
        "description": null, "display_name": "MXD Tiles", "id": 52, "learn_link": "/learn/CloudMade"
    }]
}
