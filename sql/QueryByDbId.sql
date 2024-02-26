SELECT _objects_id.id AS dbId, _objects_id.external_id AS externalId,
       _objects_attr.name AS name,_objects_attr.display_name AS propName ,
       _objects_val.value AS propValue
FROM _objects_eav
         INNER JOIN _objects_id ON _objects_eav.entity_id = _objects_id.id
         INNER JOIN _objects_attr ON _objects_eav.attribute_id = _objects_attr.id
         INNER JOIN _objects_val ON _objects_eav.value_id = _objects_val.id
where dbId = 3489
