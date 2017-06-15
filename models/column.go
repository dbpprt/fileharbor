package models

type ColumnEntity struct {
	ID           string `db:"id"`
	CollectionID string `db:"collection_id"`
	Name         string `db:"name"`
	Description  string `db:"description"`
	Group        string `db:"group"`
	Type         string `db:"type"`
	Sealed       bool   `db:"sealed"`
	Settings     string `db:"settings"`
}
