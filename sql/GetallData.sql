SELECT _objects_id.id                  AS dbId,
       _objects_id.external_id         AS externalId,
       _objects_attr.name              AS name,
       _objects_attr.category          as category,
       _objects_attr.data_type         AS dataType,
       _objects_attr.data_type_context AS dataTypeContext,
       _objects_attr.display_name      AS propName,
       _objects_val.value              AS propValue,
       _objects_attr.flags             AS flags
FROM _objects_eav
         INNER JOIN _objects_id ON _objects_eav.entity_id = _objects_id.id
         INNER JOIN _objects_attr ON _objects_eav.attribute_id = _objects_attr.id
         INNER JOIN _objects_val ON _objects_eav.value_id = _objects_val.id