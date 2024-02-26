SELECT DISTINCT
    _objects_val.value AS propValue
FROM
    _objects_eav
        INNER JOIN
    _objects_id ON _objects_eav.entity_id = _objects_id.id
        INNER JOIN
    _objects_attr ON _objects_eav.attribute_id = _objects_attr.id
        INNER JOIN
    _objects_val ON _objects_eav.value_id = _objects_val.id
WHERE
        name LIKE '_RC' ESCAPE '\' AND
                               propValue IS NOT NULL AND
                               propValue <> ''