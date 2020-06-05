<template>
<div class="news">

    <main role="main" class="col-md-9 col-lg-9 ml-auto pl-4">
        <div class="row mt-4">
            <div class="col-md-12">

                <template v-if="!error">
                    <transition name="fade-in" mode="out-in">
                        <!-- News posts -->
                        <div v-if="posts" :key="1">
                            <news-post v-for="post in posts" :key="post.id" :post="post" />
                        </div>
                    </transition>

                    <!-- Skeleton while loading posts -->
                    <div :key="2" v-if="!posts">
                        <div class="news-loading mb-5">
                            <div class="news-image"/>
                            <div class="news-h6"/>
                            <div class="news-h4"/>
                            <div class="d-flex">
                                <div class="news-p"/>
                                <div class="news-p"/>
                            </div>
                            <div class="d-flex">
                                <div class="news-p"/>
                                <div class="news-p"/>
                                <div class="news-p"/>
                            </div>
                        </div>

                        <div class="news-loading mb-5">
                            <div class="news-image"/>
                            <div class="news-h6"/>
                            <div class="news-h4"/>
                            <div class="d-flex">
                                <div class="news-p"/>
                                <div class="news-p"/>
                            </div>
                            <div class="d-flex">
                                <div class="news-p"/>
                                <div class="news-p"/>
                                <div class="news-p"/>
                            </div>
                        </div>
                    </div>
                </template>

                <template v-else>
                    <!-- Offline / Timeout -->
                    <div :key="3" class="pt-5">
                      <div class="text-center align-self-center mt-5 pt-5">
                          <h3 class="font-400 pt-5">Something went wrong</h3>
                          <p class="opacity-75">Check your internet connection and try again.</p>
                          <button type="button" name="button" class="btn btn-primary px-4 font-16 mt-4" @click="loadPosts()">Retry</button>
                      </div>
                    </div>

                </template>

            </div>
        </div>
    </main>

</div>
</template>

<style media="screen">
    .fade-in-enter-active,
    .fade-in-leave-active {
        transition: all .35s ease-in-out;
    }

    .fade-in-enter,
    .fade-in-leave-to {
        opacity: 0;
    }

    /* Animation: Blinking effect */
    @keyframes blink {
      0% {
        opacity: .1;
      }

      50% {
        opacity: .3;
      }

      100% {
        opacity: .1;
      }
    }
    
    .news-loading .news-image,
    .news-loading .news-h6,
    .news-loading .news-h4,
    .news-loading .news-p {
        animation-name: blink;
        animation-duration: 1s;
        animation-iteration-count: infinite;
        animation-fill-mode: both;
        background: var(--bg-on-dark-variant);

    }
    .news-loading .news-image {
        height: 368px;
        width: 100%;
    }
    .news-loading .news-h6 {
        height: 14px;
        width: 20%;
        margin-top: 1.5rem;
        margin-bottom: .5rem;
    }
    .news-loading .news-h4 {
        height: 28px;
        width: 80%;
        margin-top: .5rem;
        margin-bottom: .5rem;
    }
    .news-loading .news-p {
        height: 16px;
        margin-top: 0rem;
        margin-bottom: .4rem;
        width: 100%;
        margin-right: 8px;
    }
    .news-loading .news-p:first-child {
        width: 60%;
    }
    .news-loading .news-p:nth-child(2) {
        width: 40%;
    }
    .news-loading .news-p:nth-child(3) {
        width: 60%;
    }
    .news-loading .news-p:last-child {
        width: 40%;
        margin-right: 0px;
    }
</style>

<script>
/* eslint no-unused-vars:0 */

const { remote } = require('electron');
const axios = require('axios').default;

import NewsPost from '@/components/NewsPost.vue'

export default {
    name: 'Home',
    components: {
        NewsPost,
    },
    data() {
        return {
            isSettingsModalVisible: false,
            posts: '',
            error: false,
        };
    },
    methods: {
        openExternalBrowser(link) {
            remote.shell.openExternal(link);
        },
        loadPosts() {
            let that = this
            axios.get("https://nitroxblog.rux.gg/wp-json/wp/v2/posts")
                .then(function (response) {
                    that.posts = response.data;
                    that.error = false;
                })
                .catch(function (error) {
                    that.error = true;
                })
        },
        decoder(str) {
            let textArea = document.createElement("textarea");
            textArea.innerHTML = str;
            return textArea.value;
        },
    },
    mounted() {
        this.loadPosts()
    },
};
</script>
