package models

type UserCollectionMapping struct {
	UserId       string `db:"user_id"`
	CollectionId string `db:"collection_id"`
	IsDefault    bool   `db:"is_default"`
}
