package helper

import jwt "github.com/dgrijalva/jwt-go"

type Claims struct {
	ID         string `json:"id"`
	Email      string `json:"name"`
	SuperAdmin bool   `json:"superadmin"`
	jwt.StandardClaims
}
