﻿using ORM;
using ORM.Enums;

namespace Test.Models;

[Entity("accounts")]
public class Account
{
    [PrimaryGeneratedColumn]
    public int Id { get; set; }

    [Column]
    public string Nickname { get; set; }

    [OneToOne<User>(Relationship.Mandatory)]
    public User User { get; set; }
}

