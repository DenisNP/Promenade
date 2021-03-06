export const isDev = () => process.env.NODE_ENV === 'development';

export const getSearch = () => {
    const { search } = window.location;
    return new URLSearchParams(search ? search.slice(1) : '');
};

export const getUserId = () => {
    const uidGot = getSearch().get('vk_user_id') || '';
    return (!uidGot && isDev() ? '463377' : uidGot);
};

export const getAppId = () => {
    const aidGot = getSearch().get('vk_app_id') || '';
    return (!aidGot && isDev() ? '7397553' : aidGot);
};

export const getPlatform = () => {
    const p = getSearch().get('vk_platform');
    return (!p && isDev() ? 'local' : p);
};

export const getHash = () => {
    const { hash } = window.location;
    return hash ? hash.slice(1) : '';
};

export const firstUpperCase = (s) => {
    if (!s) return s;
    if (s.length === 1) return s.toUpperCase();
    return s.charAt(0).toUpperCase() + s.slice(1);
};

export const drawImage = (imageSrc, x, y, w, h) => new Promise((resolve) => {
    const img = new Image();
    img.crossOrigin = 'anonymous';
    img.onload = () => {
        const canvas = document.getElementById('canvas');
        const ctx = canvas.getContext('2d');
        if (w && h) ctx.drawImage(img, x, y, w, h);
        else ctx.drawImage(img, x, y);
        resolve();
    };
    img.src = imageSrc;
});

export const getNumericPhrase = (num, one, few, many) => {
    // eslint-disable-next-line no-param-reassign
    num = Number(num) < 0 ? 0 : Number(num);

    let postfix = '';
    if (num < 10) {
        if (num === 1) postfix = one;
        else if (num > 1 && num < 5) postfix = few;
        else postfix = many;
    } else if (num <= 20) {
        postfix = many;
    } else if (num <= 99) {
        const lastOne = num - (Math.floor(num / 10)) * 10;
        postfix = getNumericPhrase(lastOne, one, few, many);
    } else {
        const lastTwo = num - (Math.floor(num / 100)) * 100;
        postfix = getNumericPhrase(lastTwo, one, few, many);
    }
    return postfix;
};

export const ranges = [10, 15, 30, 60];

function deg2rad(deg) {
    return deg * (Math.PI / 180.0);
}

export const geoDistance = ([lon1, lat1], [lon2, lat2]) => {
    const R = 6371.01; // Radius of the Earth in km
    const dLat = deg2rad(lat2 - lat1); // deg2rad above
    const dLon = deg2rad(lon2 - lon1);
    const a = Math.sin(dLat / 2) * Math.sin(dLat / 2)
        + Math.cos(deg2rad(lat1)) * Math.cos(deg2rad(lat2))
        * Math.sin(dLon / 2) * Math.sin(dLon / 2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    return R * c; // Distance in km
};

export const shuffle = (a) => {
    for (let i = a.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        // eslint-disable-next-line no-param-reassign
        [a[i], a[j]] = [a[j], a[i]];
    }
    return a;
};
