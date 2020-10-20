const data = {
    'addr:street': 'улица',
    'addr:city': 'населённый пункт',
    'addr:housenumber': 'номер дома',
    name: 'название',
    'name:ru': 'название',
    title: 'название',
    description: 'описание',
    subject: 'описание',
    wikipedia: 'статья на Википедии',
    'subject:wikipedia': 'статья на Википедии',
    height: 'высота',
    width: 'ширина',
    length: 'длина',
    size: 'размер',
    ref: 'номер',
    'ref:okn': 'номер',
    layer: 'слой',
    operator: 'оператор',
    'building:levels': 'число этажей',
    inscription: 'информация',
    artist_name: 'автор',
    start_date: 'дата',
    date: 'дата',
};

export default (tag) => {
    const t = tag.trim().toLowerCase();
    return data[t] || 'прочее';
};
