package models

import (
	"database/sql"
	"encoding/json"
)

type CollectionEntity struct {
	ID                 string          `db:"id"`
	Quota              int64           `db:"quota"`
	BytesUsed          int64           `db:"bytes_used"`
	TemplateDefinition json.RawMessage `db:"template_definition"`
	TemplateId         sql.NullString  `db:"template_id"`
}
