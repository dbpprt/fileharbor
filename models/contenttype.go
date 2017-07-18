package models

type ContentTypeEntity struct {
	ID           string `db:"id"`
	CollectionID string `db:"collection_id"`
	Name         string `db:"name"`
	Description  string `db:"description"`
	Group        string `db:"group"`
	Sealed       bool   `db:"sealed"`
}
