package models

import (
	"database/sql"
)

type CollectionEntity struct {
	ID                    string         `db:"id"`
	Name                  sql.NullString `db:"name"`
	Description           sql.NullString `db:"description"`
	Quota                 int64          `db:"quota"`
	BytesUsed             int64          `db:"bytes_used"`
	TemplateId            sql.NullString `db:"template_id"`
	UserCollectionMapping `db:"user_collection_mapping"`
}
