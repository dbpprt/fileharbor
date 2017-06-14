
# tables

## users
- id:uuid
- email:string
- surname: string
- givenname: string
- password stuff ...

## collections
- id:uuid
- quota:int (mb)

## user collection mappings
- user_id:uuid
- collection_id:uuid

## columns
- id:uuid
- collection_id:uuid:fk
- name:string
- type:(text, int, datetime, tags)

## content types
- id:uuid
- parent_id:uuid:fk
- collection_id:uuid:fk
- name:string

## colum content type mappings
- column_id:uuid
- contenttype_id:uuid
- required:bool
- visible:bool
- default:text (expression?)

## libraries
- id:uuid
- collection_id:uuid:pk
- name:string
- logo?

## library content type mappings
- library_id:uuid:fk
- content_type_id:uuid:fk
- visible:bool

## file references
- id:uuid
- collection_id:uuid:fk
- name:string
- size:bigint
- uri:string
- type:string
- created:datetime
- updated:datetime

## item
- id:uuid
- file_reference_id:uuid:fk
- title:string
- contenttype_id:uuid:fk
- properties:json???
- created:datetime
- updated:datetime

## item attachments
- item_id:uuid:fk
- file_reference_id:uuid:fk
- title:string
