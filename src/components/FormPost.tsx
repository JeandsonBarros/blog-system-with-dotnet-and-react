import { useContext, useEffect, useState } from "react"
import Post from "../shared/models/Post"
import PostDto from "../shared/dtos/PostDto"
import { Button, Checkbox, Input, Spinner } from "@nextui-org/react"
import ImgExample from "../assets/img/img_example.png"
import FormPostStyles from "../styles/components_styles/form_post.module.css"
import { MdDelete, MdOutlineFileUpload } from "react-icons/md"
import { axiosErrorToString } from "../shared/services/API"
import { deleteCoverPost, getCoverImage } from "../shared/services/PostService"
import { MainContext } from "../App"

export default function FormPost({ action, postUpdate }: { action?: (postDto: PostDto) => Promise<void>, postUpdate?: Post }) {

    const [isLoad, setIsLoad] = useState<boolean>(false)
    const [postDto, setPostDto] = useState<PostDto>({ title: "", subtitle: "", text: "", isPublic: true, coverFile: undefined })
    const [isImageSaved, setIsImageSaved] = useState<boolean>(false)
    const [previewImage, setPreviewImage] = useState<string>(ImgExample)
    const { setAlert } = useContext(MainContext)

    useEffect(() => {

        if (postUpdate) {

            const tempPostData = {
                title: postUpdate.title,
                text: postUpdate.text,
                isPublic: postUpdate.isPublic,
                subtitle: postUpdate.subtitle
            } satisfies PostDto

            setPostDto(tempPostData)

            if (postUpdate.coverFileName) {
                setIsImageSaved(true)
                getCoverImage(postUpdate.coverFileName)
                    .then(file => {
                        setPreviewImage(URL.createObjectURL(file))
                        setPostDto({ ...tempPostData, coverFile: file })
                    })
                    .catch(error => setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true }))
            }

        }

    }, [postUpdate])

    async function executeAction() {
        if (!action) return

        if (!postDto.title || !postDto.subtitle || !postDto.text) {
            setAlert({ text: "Don't leave fields empty.", status: "warning", isVisible: true })
            return
        }

        setIsLoad(true)

        if (postUpdate && isImageSaved && !postDto.coverFile) {
            await deleteCoverPost(postUpdate.id)
            setIsImageSaved(false)
        }

        await action(postDto)

        setIsLoad(false)
    }

    return (
        <form className="rounded-lg shadow-lg p-3" style={{ maxWidth: 800 }}>

            <div className="my-3">

                <img src={previewImage} className={`${FormPostStyles.imgCover} rounded-lg`} />

                <div className="flex flex-row items-center flex-wrap">
                    <label
                        htmlFor="postCover"
                        className="m-1 flex flex-row items-center justify-center bg-cyan-700 cursor-pointer rounded-lg text-white p-2 "
                    >
                        Cover image <MdOutlineFileUpload className="ms-2" />
                    </label>

                    {postDto.coverFile &&
                        <Button
                            className="m-1"
                            variant="flat"
                            color="danger"
                            onPress={() => {
                                setPreviewImage(ImgExample)
                                setPostDto({ ...postDto, coverFile: undefined })
                            }}
                        >
                            Remove image <MdDelete />
                        </Button>
                    }
                </div>

                <input
                    id="postCover"
                    className="hidden"
                    type="file"
                    onChange={event => {

                        if (!event.target.files || event.target.files.length === 0) {
                            setPreviewImage(ImgExample)
                            setPostDto({ ...postDto, coverFile: undefined })
                            return
                        }

                        setPreviewImage(URL.createObjectURL(event.target.files[0]))
                        setPostDto({ ...postDto, coverFile: event.target.files[0] })

                        event.target.value = "";
                    }}
                />

            </div>

            <Input
                value={postDto.title}
                variant="underlined"
                placeholder="Title here"
                label="Title"
                onValueChange={value => setPostDto({ ...postDto, title: value })}
            />

            <Input
                value={postDto.subtitle}
                variant="underlined"
                placeholder="Subtitle here"
                label="Subtitle"
                onValueChange={value => setPostDto({ ...postDto, subtitle: value })}
            />

            <div className="my-3">
                <label htmlFor="textPost">Text:</label>
                <textarea
                    id="textPost"
                    placeholder="Post text here"
                    rows={19}
                    value={postDto.text}
                    className="w-full border border-gray-300 p-1 rounded-lg"
                    onChange={event => setPostDto({ ...postDto, text: event.target.value })}
                ></textarea>
            </div>

            <Checkbox
                isSelected={postDto.isPublic}
                onValueChange={value => setPostDto({ ...postDto, isPublic: value })}
            >
                Is Public
            </Checkbox>

            <hr className="my-3" />

            <Button
                onPress={executeAction}
                color="primary">
                {isLoad ? <Spinner color="default" size="sm" /> : <>Save</>}
            </Button>

        </form>
    );
}