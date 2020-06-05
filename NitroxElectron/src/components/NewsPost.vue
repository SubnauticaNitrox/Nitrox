<template>
    <div class="news-post mb-5 position-relative">
        <a href="#" class="stretched-link text-muted disable-select" draggable="false" @click.prevent="openExternalBrowser(post.link)">
            <div class="news-image-wrapper">
                <div class="news-image" :style="{'background-image': `url('${post.jetpack_featured_media_url}')`}"></div>
            </div>
        </a>
        <h6 class="mt-4">{{ prettyDate }}</h6>
        <h4 class="mt-2">{{ decoder(post.title.rendered) }}</h4>
        <p class="font-14 opacity-75">{{ decoder(post.excerpt.rendered) }}</p>
    </div>
</template>

<style media="screen">
    .news-post .news-image-wrapper {
        overflow: hidden;
        height: 368px;
    }
    .news-post .news-image {
        background-repeat: no-repeat;
        background-size: cover;
        background-position: center;
        width: 100%;
        height: 100%;
        transition: all .25s ease-in-out;
    }
    .news-post:hover .news-image {
        transform: scale(1.025);
    }
    .news-post:active .news-image {
        transform: scale(1);
    }
</style>

<script>
    const { remote } = require('electron');
    export default {
        props: ['post'],
        methods: {
            openExternalBrowser(link) {
                remote.shell.openExternal(link + '?utm_source=nitrox_launcher&utm_medium=news_article&utm_campaign=news_article');
            },
            decoder(str) {
                let parser = new DOMParser();
                let dom = parser.parseFromString('<!doctype html><body>' + str, 'text/html');
                return dom.body.textContent;
            },
        },
        computed: {
            prettyDate: function() {
                let date = new Date(this.post.date_gmt);
                let formattedDate = `${date.toLocaleString('default', {month: 'long'})} ${date.getDate()}, ${date.getFullYear()}`;
                return formattedDate;
            },
        },
    }
</script>
