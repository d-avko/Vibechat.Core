export class Cache {
    static OnUserLoggedIn(credentials) {
        this.UserCache = credentials.info;
        this.JwtToken = credentials.token;
        localStorage.setItem('token', this.JwtToken);
        localStorage.setItem('user', JSON.stringify(credentials.info));
        this.IsAuthenticated = true;
    }
    static TryAuthenticate() {
        let token = localStorage.getItem('token');
        let user = JSON.parse(localStorage.getItem('user'));
        if (token == null || user == null) {
            return false;
        }
        this.UserCache = user;
        this.JwtToken = token;
        this.IsAuthenticated = true;
        return true;
    }
    static LogOut() {
        localStorage.clear();
    }
}
//# sourceMappingURL=Cache.js.map