package context

import (
	"github.com/jmoiron/sqlx"
	"github.com/labstack/echo"
)

type Context struct {
	echo.Context

	database *sqlx.DB
}

func New(c *echo.Context, db *sqlx.DB) (*Context, error) {
	ctx := &Context{Context: *c, database: db}
	return ctx, nil
}

func (c *Context) Database() *sqlx.DB {
	return c.database
}
