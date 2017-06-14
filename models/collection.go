package models

type CollectionEntity struct {
	ID        string `db:"id"`
	Quota     int64  `db:"quota"`
	BytesUsed int64  `db:"bytes_used"`
}
