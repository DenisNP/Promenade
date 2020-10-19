const data = {
    'addr:street': 'улица',
    name: 'название',
    'name:ru': 'название',
    title: 'название',
    description: 'описание',
    subject: 'описание',
    inscription: 'информация',
};

export default (tag) => {
    const t = tag.trim().toLowerCase();
    return data[t] || tag;
};
