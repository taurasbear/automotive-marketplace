import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import {
  RegisterSchema,
  setCredentials,
  useRegisterUser,
} from "@/features/auth";
import { useAppDispatch } from "@/hooks/redux";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link, useNavigate } from "@tanstack/react-router";
import { useForm } from "react-hook-form";
import { z } from "zod";

const Register = () => {
  const { mutateAsync: registerUserAsync } = useRegisterUser();
  const navigate = useNavigate();

  const dispatch = useAppDispatch();

  const form = useForm({
    resolver: zodResolver(RegisterSchema),
    defaultValues: {
      username: "",
      email: "",
      password: "",
    },
  });

  const onSubmit = async (formData: z.infer<typeof RegisterSchema>) => {
    const { data: user } = await registerUserAsync({
      username: formData.username,
      email: formData.email,
      password: formData.password,
    });

    dispatch(
      setCredentials({
        accessToken: user.accessToken,
        permissions: [],
        userId: user.userId,
      }),
    );

    await navigate({ to: "/" });
  };

  return (
    <div className="flex min-h-screen flex-col items-center justify-center">
      <h2 className="mb-6 text-2xl font-bold">Register</h2>
      <Form {...form}>
        <form
          onSubmit={form.handleSubmit(onSubmit)}
          className="w-full max-w-md space-y-6"
        >
          <FormField
            control={form.control}
            name="username"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Username</FormLabel>
                <FormControl>
                  <Input placeholder="Your username" {...field} />
                </FormControl>
                <FormDescription>
                  This will be your public display name.
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="email"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Email</FormLabel>
                <FormControl>
                  <Input
                    type="email"
                    placeholder="you@example.com"
                    {...field}
                  />
                </FormControl>
                <FormDescription>{`We'll never share your email.`}</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="password"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Password</FormLabel>
                <FormControl>
                  <Input type="password" placeholder="Password" {...field} />
                </FormControl>
                <FormDescription>At least 6 characters.</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
          <Button type="submit" className="w-full">
            Register
          </Button>
        </form>
      </Form>
      <Link to="/login">
        <Button variant="link" className="m-4">
          Already have an account?
        </Button>
      </Link>
    </div>
  );
};

export default Register;
