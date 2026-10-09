const cookieName = '.AspNetCore.Culture';

// The next HTTP request restores this pair before SSR; interactive changes need no navigation.
export function saveCulture(culture, formatCulture, basePath, maxAge) {
    if ([culture, formatCulture].some(value => typeof value !== 'string' || !value || /[|;=\r\n]/.test(value)))
        throw new TypeError('Invalid culture selection.');
    if (typeof basePath !== 'string' || !basePath.startsWith('/') || /[;\r\n]/.test(basePath)
        || !Number.isSafeInteger(maxAge) || maxAge <= 0 || maxAge > 3650 * 86400)
        throw new TypeError('Invalid preference cookie options.');
    const value = encodeURIComponent(`c=${formatCulture}|uic=${culture}`);
    const secure = location.protocol === 'https:' ? '; secure' : '';
    document.cookie = `${cookieName}=${value}; path=${basePath}; max-age=${maxAge}; samesite=lax${secure}`;
    if (!document.cookie.split(';').some(part => part.trim() === `${cookieName}=${value}`))
        throw new Error('The browser could not save the language preference.');
}
