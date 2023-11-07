import { Input } from "@nextui-org/react";
import { useState } from "react";
import { MdVisibility, MdVisibilityOff } from "react-icons/md";

interface InputPasswordProps {
    setValue?: (value: string) => void,
    label?: string
}

function InputPassword({ label, setValue }: InputPasswordProps) {

    const [isVisiblePassword, setIsVisiblePassword] = useState(false);

    const toggleVisibility = () => setIsVisiblePassword(!isVisiblePassword);

    return (
        <Input
            variant="underlined"
            label={label}
            endContent={
                <button className="focus:outline-none" type="button" onClick={toggleVisibility}>
                    {isVisiblePassword ? (
                        <MdVisibilityOff className="text-2xl text-default-400 pointer-events-none" />
                    ) : (
                        <MdVisibility className="text-2xl text-default-400 pointer-events-none" />
                    )}
                </button>
            }
            type={isVisiblePassword ? "text" : "password"}
            onChange={value => setValue? setValue(value.target.value): console.log("Enter a function to insert the value") }
        />
    );
}

export default InputPassword;