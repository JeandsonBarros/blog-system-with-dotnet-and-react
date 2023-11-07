import { Modal, ModalContent, ModalHeader, ModalBody, Checkbox, ModalFooter, Button, Spinner, Input, Textarea } from "@nextui-org/react"
import { useState, useEffect } from "react"
import BlogDto from "../shared/dtos/BlogDto"
import Blog from "../shared/models/Blog"

interface ModalBlogProps {
    isOpen: boolean,
    setIsOpen: (isOpen: boolean) => void,
    blog?: Blog,
    action: (blogDto: BlogDto) => Promise<void>
}
export default function ModalBlog({ isOpen, setIsOpen, blog, action }: ModalBlogProps) {

    const [isLoad, setIsLoad] = useState<boolean>(false)
    const [blogDto, setBlogDto] = useState<BlogDto>({ title: "", description: "", isPublic: true })

    useEffect(() => {
        if (blog) {
            setBlogDto({
                title: blog.title,
                description: blog.description,
                isPublic: blog.isPublic,
                headerColor: blog.headerColor,
                titleColor: blog.titleColor
            })
        }
    }, [blog])

    return (
        <Modal isOpen={isOpen} disableAnimation onOpenChange={() => setIsOpen(!isOpen)}>

            <ModalContent className="h-fit">

                <ModalHeader className="flex flex-col gap-1">
                    {blog ? "Update blog" : "Create blog"}
                </ModalHeader>

                <ModalBody >

                    <Input
                        value={blogDto.title}
                        variant="underlined"
                        label="Title"
                        placeholder="Enter the blog title"
                        onValueChange={value => setBlogDto({ ...blogDto, title: value })}
                    />

                    <Checkbox
                        isSelected={blogDto.isPublic}
                        onValueChange={(value) => setBlogDto({ ...blogDto, isPublic: value })}
                    >
                        Is Public
                    </Checkbox>

                    <div className="flex flex-row items-center">
                        <label htmlFor="headerColor">Header color: </label>
                        <input
                            type="color"
                            id="headerColor"
                            value={blogDto.headerColor}
                            onChange={event => setBlogDto({ ...blogDto, headerColor: event.target.value })}
                        />
                    </div>

                    <div className="flex flex-row items-center">
                        <label htmlFor="titleColor">Title color: </label>
                        <input
                            type="color"
                            id="titleColor"
                            value={blogDto.titleColor}
                            onChange={event => setBlogDto({ ...blogDto, titleColor: event.target.value })}
                        />
                    </div>

                    <Textarea
                        value={blogDto.description}
                        onValueChange={value => setBlogDto({ ...blogDto, description: value })}
                        label="Description"
                        labelPlacement="outside"
                        placeholder="Enter the blog description"
                    />

                </ModalBody>

                <ModalFooter>

                    <Button color="danger" variant="light" onPress={() => setIsOpen(false)}>
                        Cancel
                    </Button>

                    <Button
                        onPress={async () => {
                            setIsLoad(true)
                            await action(blogDto)
                            setIsLoad(false)
                        }}
                        color="primary">
                        {isLoad ? <Spinner color="default" size="sm" /> : <>Save</>}
                    </Button>

                </ModalFooter>

            </ModalContent>
        </Modal>
    )
}