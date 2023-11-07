import {
    Button,
    Dropdown,
    DropdownItem,
    DropdownMenu,
    DropdownTrigger,
    Input,
    Modal,
    ModalBody,
    ModalContent,
    ModalFooter,
    ModalHeader,
    Progress,
    Spinner,
} from '@nextui-org/react';
import { useContext, useEffect, useState } from 'react';
import { MdExpandMore, MdInfo, MdOutlineMoreVert, MdSend } from 'react-icons/md';
import { useParams } from 'react-router-dom';

import { MainContext } from '../../App';
import UserImgExample from '../../assets/img/person-circle.svg';
import PostItem from '../../components/PostCard';
import Comment from '../../shared/models/Comment';
import Post from '../../shared/models/Post';
import User from '../../shared/models/User';
import { axiosErrorToString, baseURL } from '../../shared/services/API';
import { getDataAccount } from '../../shared/services/AuthService';
import { deleteComment, getCommentsPost, postComment, putComment } from '../../shared/services/CommentService';
import { getCoverImage, getPublicPost, getPublicPostsByBlog, getUserPost } from '../../shared/services/PostService';
import FormPostStyles from '../../styles/components_styles/form_post.module.css';

export default function PostView() {

    const { setAlert } = useContext(MainContext)
    const params = useParams()
    const [post, setPost] = useState<Post>()
    const [isLoad, setIsLoad] = useState<boolean>(false)
    const [urlCoverImg, setUrlCoverImg] = useState<string>()
    const [isPreview, setIsPreview] = useState<boolean>(false)

    useEffect(() => {
        window.scrollTo(0, 0)
        getPostData()
    }, [params])

    useEffect(() => {

        if (post?.coverFileName) {
            getCoverImage(post.coverFileName)
                .then(file => setUrlCoverImg(URL.createObjectURL(file)))
                .catch(error => setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true }))
        }

    }, [post])

    async function getPostData(): Promise<void> {

        setIsLoad(true)

        try {

            const pathnameSplit = location.pathname.split("/")
            const isPreviewTemp = pathnameSplit[pathnameSplit.length - 1] == "preview" && pathnameSplit.length == 7
            const response = isPreviewTemp ? await getUserPost(Number(params.postId)) : await getPublicPost(Number(params.postId))
            setPost(response.data)
            setIsPreview(isPreviewTemp)

        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }

        setIsLoad(false)

    }

    return (
        <section className="flex flex-col items-center max-w-7xl m-auto">

            {isLoad &&
                <Progress
                    size="sm"
                    isIndeterminate
                    aria-label="Loading..."
                    className="w-full"
                />
            }

            {isPreview &&
                <p className='flex flex-row items-center ps-2 text-xl h-10 bg-sky-600 text-white z-40 w-full '>
                   <MdInfo className="me-1"/> Post Preview
                </p>
            }

            {urlCoverImg &&
                <img
                    src={urlCoverImg}
                    className={`${FormPostStyles.imgCover} absolute blur-sm`}
                />
            }

            {post &&
                <>
                    <div className="p-4 mt-7 m-2 rounded-lg shadow-md z-10 bg-white w-full" style={{ maxWidth: 800 }}>

                        {urlCoverImg &&
                            <img
                                src={urlCoverImg}
                                className={`${FormPostStyles.imgCover} rounded-lg`}
                            />
                        }

                        <h1 className="text-2xl mt-2">{post.title}</h1>

                        <h2 className="text-slate-500">{post.subtitle}</h2>

                        <small className="text-slate-500">
                            In {(() => (new Date(post.date).toLocaleDateString("en-US")))()}
                            {post.isUpdated && <> - Edited</>}
                        </small>

                        <p className="mt-5">{post.text}</p>

                    </div>

                    <PostComments post={post} />

                    <MorePosts />
                </>
            }

        </section >
    );
}

function PostComments({ post }: { post: Post }) {

    const { setAlert } = useContext(MainContext)
    const [isLoad, setIsLoad] = useState<boolean>(false)
    const [comments, setComments] = useState<Comment[]>([])
    const [userAuth, setUserAuth] = useState<User>()
    const [userComment, setUserComment] = useState<string>("")
    const [pagination, setPagination] = useState<any>({ totalPages: 1, actualPage: 1, totalRecords: 0 })

    useEffect(() => {

        if (localStorage.getItem("token")) {
            getDataAccount()
                .then(response => setUserAuth(response.data))
                .catch(error => setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true }))
        }

        listComments()

    }, [post])

    async function listComments(page = 1): Promise<void> {
        setIsLoad(true)
        try {

            const response = await getCommentsPost(post.id, page)

            setComments(page == 1 ? response.data : comments.concat(response.data))

            setPagination({
                totalPages: response.totalPages,
                actualPage: response.page,
                totalRecords: response.totalRecords
            })

        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }
        setIsLoad(false)
    }

    async function createComment(): Promise<void> {
        setIsLoad(true)
        try {
            const response = await postComment(post.id, userComment)
            setAlert({ text: "Comment saved", status: "success", isVisible: true })

            response.data.user = userAuth
            const tempComments = [...comments]
            tempComments.push(response.data)
            setComments(tempComments)

        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }
        setIsLoad(false)
    }

    return (
        <div className="p-4 mt-7 m-2 rounded-lg shadow-md bg-white w-full" style={{ maxWidth: 800 }}>

            <h1 className="text-2xl mb-3">Comments</h1>

            <hr />

            <Input
                className="mt-3"
                variant="underlined"
                placeholder="Your comment here"
                onValueChange={setUserComment}
                endContent={
                    <Button
                        isIconOnly
                        isDisabled={userComment.length == 0}
                        variant="light"
                        onPress={createComment}
                    >
                        <MdSend className="text-xl" />
                    </Button>
                }
            />

            {isLoad &&
                <Progress
                    size="sm"
                    isIndeterminate
                    aria-label="Loading..."
                    className="w-full mt-3"
                />
            }

            {(!isLoad && comments.length == 0) &&
                <p className="mt-3">Nem uma comentário encontrado.</p>
            }

            <div className="mt-5 flex flex-col items-center">

                {comments.map(comment => (
                    <CommentItem
                        key={comment.id}
                        comment={comment}
                        comments={comments}
                        setComments={setComments}
                        userAuth={userAuth}
                    />
                ))}

                {(comments.length > 0 && comments.length < pagination.totalRecords) &&
                    <Button
                        className="m-auto"
                        isIconOnly
                        title="Show more"
                        variant="light"
                        color="default"
                        radius="full"
                        onPress={() => listComments(pagination.actualPage + 1)}
                    >
                        <MdExpandMore className="text-xl" />
                    </Button>
                }

            </div>

        </div>
    )
}

interface CommentItemProps {
    comment: Comment
    comments: Comment[]
    setComments: (comments: Comment[]) => void
    userAuth?: User
}
function CommentItem({ comment, comments, setComments, userAuth }: CommentItemProps) {

    const [commentText, setCommentText] = useState<string>("")
    const [isOpenModalUpdate, setIsOpenModalUpdate] = useState<boolean>(false)
    const [isLoad, setIsLoad] = useState<boolean>(false)
    const { setAlert } = useContext(MainContext)

    useEffect(() => { setCommentText(comment.commentText) }, [comment])

    async function updateComment(): Promise<void> {

        setIsLoad(true)

        try {

            const response = await putComment(comment.id, commentText)
            const index = comments.map(commentItem => commentItem.id).indexOf(comment.id);

            comments[index] = response.data
            comments[index].user = userAuth

            setComments([...comments])
            setIsOpenModalUpdate(false)

        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }

        setIsLoad(false)

    }

    async function removeComment(): Promise<void> {

        setIsLoad(true)

        try {

            await deleteComment(comment.id)

            const index = comments.map(commentItem => commentItem.id).indexOf(comment.id);
            comments.splice(index, 1);
            setComments([...comments])

        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }

        setIsLoad(false)

    }

    return (
        <div className="mb-3 w-full">

            <div className="flex flex-row items-center justify-between">

                <div className="flex flex-row items-center">
                    <img
                        className="w-10 h-10 rounded-full object-cover me-2"
                        src={comment.user?.fileProfilePictureName
                            ? `${baseURL}/auth/profile-picture/${comment.user.fileProfilePictureName}`
                            : UserImgExample
                        }
                        alt=""
                    />

                    <span>{comment.user?.name}</span>
                </div>

                {(userAuth && userAuth.id == comment.user?.id) &&
                    <>
                        <Dropdown>
                            <DropdownTrigger>
                                <Button
                                    isIconOnly
                                    variant="light"
                                    aria-label="Like"
                                >
                                    <MdOutlineMoreVert />
                                </Button>
                            </DropdownTrigger>
                            <DropdownMenu aria-label="Static Actions">
                                <DropdownItem
                                    key="update"
                                    onPress={() => setIsOpenModalUpdate(true)}
                                >
                                    Update
                                </DropdownItem>
                                <DropdownItem
                                    key="delete"
                                    className="text-danger"
                                    color="danger"
                                    onPress={removeComment}
                                >
                                    Delete
                                </DropdownItem>
                            </DropdownMenu>
                        </Dropdown>

                        <Modal
                            isOpen={isOpenModalUpdate}
                            disableAnimation
                            onOpenChange={() => setIsOpenModalUpdate(!isOpenModalUpdate)}
                        >

                            <ModalContent className="h-fit">

                                <ModalHeader className="flex flex-col gap-1">
                                    Update comment
                                </ModalHeader>

                                <ModalBody >

                                    <Input
                                        value={commentText}
                                        variant="underlined"
                                        placeholder="Your comment here"
                                        onValueChange={setCommentText}
                                    />

                                </ModalBody>

                                <ModalFooter>

                                    <Button color="danger" variant="light" onPress={() => setIsOpenModalUpdate(false)}>
                                        Cancel
                                    </Button>

                                    <Button
                                        onPress={updateComment}
                                        color="primary">
                                        {isLoad ? <Spinner color="default" size="sm" /> : <>Save</>}
                                    </Button>

                                </ModalFooter>

                            </ModalContent>
                        </Modal>
                    </>
                }

            </div>

            <p>{comment.commentText}</p>

            <small className="text-slate-500">
                In {(() => (new Date(comment.date).toLocaleDateString("en-US")))()}
                {comment.isUpdated && <> - Edited</>}
            </small>

        </div>

    )
}

function MorePosts() {

    const { setAlert } = useContext(MainContext)
    const params = useParams()
    const [posts, setPosts] = useState<Post[]>([])
    const [isLoad, setIsLoad] = useState<boolean>(false)

    useEffect(() => {
        window.scrollTo(0, 0)
        listMorePosts()
    }, [params])

    async function listMorePosts(): Promise<void> {

        setIsLoad(true)

        try {

            const response = await getPublicPostsByBlog(Number(params.blogId), 1, 4)
            const index = response.data.map(postItem => postItem.id).indexOf(Number(params.postId));

            response.data.splice(index, 1);
            setPosts(response.data)

        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }

        setIsLoad(false)

    }

    return (
        <div className="flex flex-col p-4 mt-7 m-2 rounded-lg shadow-md bg-white w-full" style={{ maxWidth: 800 }}>

            <h1 className="text-2xl mb-3">More posts</h1>

            <hr />

            {isLoad &&
                <Progress
                    size="sm"
                    isIndeterminate
                    aria-label="Loading..."
                    className="w-full mt-3"
                />
            }

            {(!isLoad && posts.length == 0) &&
                <p className="mt-3">Nem uma postagem encontrada.</p>
            }

            <div className="m-2 flex flex-row flex-wrap justify-center w-full">
                {posts.map(post => (
                <PostItem 
                key={post.id} 
                post={post} 
                blogTitle={post.blog.title} 
                classStyles='m-3'
                />
                ))}
            </div>

        </div>
    )
}