package common

const (
	ErrNotFound            = "err-not-found"
	ErrUserAlreadyExisting = "err-user-already-existing"
	ErrUnexpected          = "err-unexpected"
	ErrPasswordTooShort    = "err-password-too-short"
	ErrLoginFailed         = "err-login-failed"
)

type ApplicationError struct {
	msg  string
	Code string
}

func (e *ApplicationError) Error() string { return e.msg }

func NewApplicationError(msg string, code string) *ApplicationError {
	return &ApplicationError{
		msg:  msg,
		Code: code,
	}
}
