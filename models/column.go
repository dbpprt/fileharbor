package models

type ColumnEntity struct {
	ID          string `db:"id"`
	Name        string `db:"name"`
	Description string `db:"description"`
	Group       string `db:"groups"`
	Type        string `db:"type"`
	Sealed      bool   `db:"sealed"`
	Settings    string `db:"settings"`
}
