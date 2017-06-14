
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
- name:string
- type:(text, int, datetime, tags)