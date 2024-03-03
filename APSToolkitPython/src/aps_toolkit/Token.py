class Token():
    def __init__(self, access_token, token_type, expires_in, refresh_token=None):
        self.access_token = access_token
        self.token_type = token_type
        self.expires_in = expires_in
        self.refresh_token = refresh_token
